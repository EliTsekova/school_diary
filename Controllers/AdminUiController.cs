using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using school_diary.Data;
using school_diary.Dtos;
using school_diary.Models;
using school_diary.Services;
using school_diary.ViewModels;
using school_diary.Views.AdminUi;

namespace school_diary.Controllers;

[Authorize(Roles = "Admin")]
[Route("AdminUi/[action]")]
public class AdminUiController : Controller
{
    private readonly IParentService _parents;
    private readonly IDirectorService _directors;
    private readonly ISchoolService _schools;
    private readonly ISubjectService _subjects;
    private readonly ITeacherService _teachers;
    private readonly ICurriculumService _curricula;
    private readonly IStudentService _students;
    private readonly IAdminStatisticsService _stats;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AdminUiController(
        IParentService parents,
        IDirectorService directors,
        ISchoolService schools,
        ISubjectService subjects,
        ITeacherService teachers,
        ICurriculumService curricula,
        IStudentService students,
        IAdminStatisticsService stats,
        ApplicationDbContext db,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _parents = parents;
        _directors = directors;
        _schools = schools;
        _subjects = subjects;
        _teachers = teachers;
        _curricula = curricula;
        _students = students;
        _stats = stats;
        _db = db;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.StudentsCount = await _db.Students.CountAsync();
        ViewBag.TeachersCount = await _db.Teachers.CountAsync();
        ViewBag.ParentsCount = await _db.Parents.CountAsync();
        ViewBag.SchoolsCount = await _db.Schools.CountAsync();

        return View();
    }

    public async Task<IActionResult> Students()
    {
        var model = await _students.GetAllAsync();

        return View("Students", model);
    }

    [HttpGet]
    public async Task<IActionResult> AddStudent()
    {
        var model = new AddStudentViewModel
        {
            Schools = await _db.Schools
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync(),

            Classes = await _db.Classes
                .OrderBy(c => c.Id)
                .Select(c => new SelectListItem
                {
                    Value = c.Name,
                    Text = c.Name
                })
                .ToListAsync()
        };

        return View("~/Views/Student/AddStudent.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudent(AddStudentViewModel model)
    {
        model.Schools = await _db.Schools
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            })
            .ToListAsync();

        model.Classes = await _db.Classes
            .Select(c => new SelectListItem
            {
                Value = c.Name,
                Text = c.Name
            })
            .ToListAsync();

        if (!ModelState.IsValid)
            return View("~/Views/Student/AddStudent.cshtml", model);

        await _students.CreateAsync(new CreateStudentDto
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Password = model.Password,
            SchoolId = model.SelectedSchoolId,
            ClassName = model.SelectedClassName
        });

        TempData["Success"] = "The student has been added successfully.";

        return RedirectToAction(nameof(Students));
    }

    [HttpGet]
    public async Task<IActionResult> EditStudent(int id)
    {
        var student = await _db.Students
            .Include(s => s.User)
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound();

        var model = new EditStudentViewModel
        {
            Id = student.Id,
            FirstName = student.User.FirstName,
            LastName = student.User.LastName,
            Email = student.User.Email ?? "",
            SelectedSchoolId = student.SchoolId,
            SelectedClassName = student.Class != null ? student.Class.Name : "",

            Schools = await _db.Schools
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync(),

            Classes = await _db.Classes
                .OrderBy(c => c.Id)
                .Select(c => new SelectListItem
                {
                    Value = c.Name,
                    Text = c.Name
                })
                .ToListAsync()
        };

        return View("~/Views/Student/EditStudent.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStudent(EditStudentViewModel model)
    {
        var student = await _db.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == model.Id);

        if (student == null)
            return NotFound();

        model.Schools = await _db.Schools
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            })
            .ToListAsync();

        model.Classes = await _db.Classes
            .OrderBy(c => c.Id)
            .Select(c => new SelectListItem
            {
                Value = c.Name,
                Text = c.Name
            })
            .ToListAsync();

        if (!ModelState.IsValid)
            return View("~/Views/Student/EditStudent.cshtml", model);

        var selectedClass = await _db.Classes
            .FirstOrDefaultAsync(c => c.Name == model.SelectedClassName);

        if (selectedClass == null)
        {
            ModelState.AddModelError("", "The selected class does not exist.");
            return View("~/Views/Student/EditStudent.cshtml", model);
        }

        student.User.FirstName = model.FirstName;
        student.User.LastName = model.LastName;
        student.User.Email = model.Email;
        student.User.UserName = model.Email;

        student.SchoolId = model.SelectedSchoolId;
        student.ClassId = selectedClass.Id;

        var updateResult = await _userManager.UpdateAsync(student.User);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError("", error.Description);

            return View("~/Views/Student/EditStudent.cshtml", model);
        }

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(student.User);
            var passwordResult = await _userManager.ResetPasswordAsync(student.User, token, model.NewPassword);

            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("~/Views/Student/EditStudent.cshtml", model);
            }
        }

        await _db.SaveChangesAsync();

        TempData["Success"] = "The student has been updated successfully.";

        return RedirectToAction(nameof(Students));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var student = await _db.Students
            .Include(s => s.User)
            .Include(s => s.Grades)
            .Include(s => s.Absences)
            .Include(s => s.ParentStudents)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound();

        var user = student.User;

        _db.ParentStudents.RemoveRange(student.ParentStudents);
        _db.Grades.RemoveRange(student.Grades);
        _db.Absences.RemoveRange(student.Absences);
        _db.Students.Remove(student);

        await _db.SaveChangesAsync();

        if (user != null)
            await _userManager.DeleteAsync(user);

        return RedirectToAction(nameof(Students));
    }

    public async Task<IActionResult> Teachers()
    {
        var allSubjects = await _subjects.GetAllAsync();
        var allClasses = await _db.Classes.AsNoTracking().ToListAsync();

        var model = (await _teachers.GetAllAsync())
            .Select(t => new TeacherRowVm
            {
                Id = t.Id,
                FullName = t.FullName,
                Email = t.Email,
                SchoolId = t.SchoolId,

                SubjectNames = allSubjects
                    .Where(s => t.SubjectIds.Contains(s.Id))
                    .Select(s => s.Name)
                    .ToList(),

                ClassNames = allClasses
                    .Where(c => t.ClassIds.Contains(c.Id))
                    .Select(c => c.Name)
                    .ToList()
            })
            .ToList();

        return View("Teachers/Index", model);
    }

    [HttpGet]
    public async Task<IActionResult> AddTeacher()
    {
        await LoadTeacherLists();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTeacher(CreateTeacherDto input)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                await LoadTeacherLists(input.SubjectIds, input.ClassIds, input.SchoolId);
                return View(input);
            }

            await _teachers.CreateAsync(input);

            TempData["Success"] = "The teacher has been added successfully.";

            return RedirectToAction(nameof(Teachers));
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError("", "A user with this email already exists.");
            await LoadTeacherLists(input.SubjectIds, input.ClassIds, input.SchoolId);
            return View(input);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            await LoadTeacherLists(input.SubjectIds, input.ClassIds, input.SchoolId);
            return View(input);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditTeacher(int id)
    {
        var teacher = await _db.Teachers
            .Include(t => t.TeacherSubjects)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            return NotFound();

        var user = await _userManager.FindByIdAsync(teacher.UserId);

        if (user == null)
            return NotFound();

        var model = new CreateTeacherDto(
            user.FirstName,
            user.LastName,
            user.Email ?? "",
            "DummyPassword123!",
            teacher.SchoolId,
            teacher.TeacherSubjects.Select(ts => ts.SubjectId).ToList(),
            ParseClassIds(teacher.AssignedClasses)
        );

        ViewBag.Id = id;

        await LoadTeacherLists(model.SubjectIds, model.ClassIds, model.SchoolId);

        return View("~/Views/AdminUi/Teachers/Edit.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTeacher(int id, CreateTeacherDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Id = id;
            await LoadTeacherLists(model.SubjectIds, model.ClassIds, model.SchoolId);
            return View("~/Views/AdminUi/Teachers/Edit.cshtml", model);
        }

        var dto = new UpdateTeacherDto(
            model.FirstName,
            model.LastName,
            model.Email,
            model.SchoolId,
            model.SubjectIds,
            model.ClassIds,
            null
        );

        await _teachers.UpdateAsync(id, dto);

        TempData["Success"] = "The teacher has been updated successfully.";

        return RedirectToAction(nameof(Teachers));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTeacher(int id)
    {
        await _teachers.DeleteAsync(id);

        return RedirectToAction(nameof(Teachers));
    }

    public async Task<IActionResult> Parents()
    {
        var parents = await _parents.GetAllAsync();

        var rows = new List<ParentRowVm>();

        foreach (var parent in parents)
        {
            var studentNames = await _parents.GetStudentNamesForParentAsync(parent.Id);

            rows.Add(new ParentRowVm
            {
                Id = parent.Id,
                FullName = parent.FullName,
                Email = parent.Email,
                StudentNames = studentNames ?? new List<string>()
            });
        }

        var model = new ParentsIndexVm
        {
            Parents = rows
        };

        return View("Parents/Index", model);
    }

    [HttpGet]
    public async Task<IActionResult> AddParent()
    {
        var model = new AddParentModel.ParentInputModel();

        ViewBag.Students = await GetStudentSelectListAsync();

        return View("~/Views/AdminUi/AddParent.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddParent(AddParentModel.ParentInputModel input)
    {
        ViewBag.Students = await GetStudentSelectListAsync();

        if (!ModelState.IsValid)
            return View("~/Views/AdminUi/AddParent.cshtml", input);

        try
        {
            var dto = new CreateParentDto(
                input.FirstName,
                input.LastName,
                input.Email,
                input.Password,
                input.SelectedStudentIds ?? new List<int>()
            );

            await _parents.CreateAsync(dto);

            TempData["Success"] = "The parent has been added successfully.";

            return RedirectToAction(nameof(Parents));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("~/Views/AdminUi/AddParent.cshtml", input);
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditParent(int id)
    {
        var parent = await _parents.GetAsync(id);

        if (parent == null)
            return NotFound();

        var names = parent.FullName.Split(' ', 2);

        var selectedStudents = await _parents.GetStudentIdsForParentAsync(id);

        ViewBag.Id = id;

        await LoadStudents(selectedStudents);

        return View("Parents/EditParent", new UpdateParentDto(
            names.ElementAtOrDefault(0) ?? "",
            names.ElementAtOrDefault(1) ?? "",
            parent.Email
        ));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditParent(
        int id,
        UpdateParentDto input,
        List<int> studentIds)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Id = id;
            await LoadStudents(studentIds);
            return View("Parents/EditParent", input);
        }

        try
        {
            await _parents.UpdateAsync(id, input);
            await _parents.AssignStudentsAsync(id, studentIds);

            TempData["Success"] = "The parent has been updated successfully.";

            return RedirectToAction(nameof(Parents));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);

            ViewBag.Id = id;
            await LoadStudents(studentIds);

            return View("Parents/EditParent", input);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteParent(int id)
    {
        await _parents.DeleteAsync(id);

        return RedirectToAction(nameof(Parents));
    }

    public async Task<IActionResult> Schools()
    {
        return View("Schools/Index", await _schools.GetAllAsync());
    }

    [HttpGet]
    public IActionResult AddSchool()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSchool(CreateSchoolDto input)
    {
        if (!ModelState.IsValid)
            return View(input);

        var exists = await _db.Schools
            .AnyAsync(s => s.Name == input.Name && s.Address == input.Address);

        if (exists)
        {
            ModelState.AddModelError("", "School already exists.");
            return View(input);
        }

        await _schools.CreateAsync(input);

        return RedirectToAction(nameof(Schools));
    }

    [HttpGet]
    public async Task<IActionResult> EditSchool(int id)
    {
        var school = await _schools.GetAsync(id);

        if (school == null)
            return NotFound();

        ViewBag.Id = id;

        return View("Schools/Edit", new UpdateSchoolDto
        {
            Name = school.Name,
            Address = school.Address
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSchool(int id, UpdateSchoolDto input)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Id = id;
            return View("Schools/Edit", input);
        }

        var exists = await _db.Schools
            .AnyAsync(s => s.Id != id &&
                           s.Name == input.Name &&
                           s.Address == input.Address);

        if (exists)
        {
            ViewBag.Id = id;

            ModelState.AddModelError("", "School already exists.");

            return View("Schools/Edit", input);
        }

        await _schools.UpdateAsync(id, input);

        return RedirectToAction(nameof(Schools));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSchool(int id)
    {
        try
        {
            await _schools.DeleteAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Schools));
    }

    public async Task<IActionResult> Subjects()
    {
        return View("Subjects/Index", await _subjects.GetAllAsync());
    }

    [HttpGet]
    public IActionResult AddSubject()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSubject(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "Name is required.");
            return View();
        }

        _db.Subjects.Add(new Subject
        {
            Name = name.Trim()
        });

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Subjects));
    }

    [HttpGet]
    public async Task<IActionResult> EditSubject(int id)
    {
        var subject = await _db.Subjects.FindAsync(id);

        if (subject == null)
            return NotFound();

        return View("Subjects/Edit", new SubjectDto(subject.Id, subject.Name));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSubject(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "Name is required.");
            return RedirectToAction(nameof(EditSubject), new { id });
        }

        var subject = await _db.Subjects.FindAsync(id);

        if (subject == null)
            return NotFound();

        subject.Name = name;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Subjects));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSubject(int id)
    {
        var subject = await _db.Subjects.FindAsync(id);

        if (subject == null)
            return NotFound();

        var isUsed = await _db.TeacherSubjects.AnyAsync(ts => ts.SubjectId == id)
                     || await _db.CurriculumEntries.AnyAsync(ce => ce.SubjectId == id)
                     || await _db.Grades.AnyAsync(g => g.SubjectId == id)
                     || await _db.Absences.AnyAsync(a => a.SubjectId == id);

        if (isUsed)
        {
            TempData["Error"] = "Cannot delete subject because it is already used in teachers, curricula, grades or absences.";
            return RedirectToAction(nameof(Subjects));
        }

        _db.Subjects.Remove(subject);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Subject deleted successfully.";

        return RedirectToAction(nameof(Subjects));
    }

    public async Task<IActionResult> Directors()
    {
        return View("Directors/Index", await _directors.GetAllAsync());
    }

    [HttpGet]
    public async Task<IActionResult> AddDirector()
    {
        ViewBag.Schools = new SelectList(await _schools.GetAllAsync(), "Id", "Name");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDirector(
        string firstName,
        string lastName,
        string email,
        string password,
        int schoolId)
    {
        ViewBag.Schools = new SelectList(await _schools.GetAllAsync(), "Id", "Name", schoolId);

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            schoolId <= 0)
        {
            ModelState.AddModelError("", "All fields are required.");
            return View();
        }

        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser != null)
        {
            ModelState.AddModelError("", "A user with this email already exists.");
            return View();
        }

        try
        {
            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = Role.Director,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View();
            }

            await _userManager.AddToRoleAsync(user, "Director");

            await _directors.CreateAsync(new CreateDirectorDto
            {
                UserId = user.Id,
                SchoolId = schoolId
            });

            return RedirectToAction(nameof(Directors));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> EditDirector(int id)
    {
        var director = await _db.Directors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (director == null)
            return NotFound();

        ViewBag.Id = id;
        ViewBag.Schools = new SelectList(await _schools.GetAllAsync(), "Id", "Name", director.SchoolId);

        return View("Directors/Edit", new UpdateDirectorDto(
            director.User.FirstName,
            director.User.LastName,
            director.User.Email ?? "",
            director.SchoolId
        ));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDirector(int id, UpdateDirectorDto input)
    {
        ViewBag.Id = id;
        ViewBag.Schools = new SelectList(await _schools.GetAllAsync(), "Id", "Name", input.SchoolId);

        if (!ModelState.IsValid)
            return View("Directors/Edit", input);

        try
        {
            await _directors.UpdateAsync(id, input);

            TempData["Success"] = "The director has been updated successfully.";

            return RedirectToAction(nameof(Directors));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("Directors/Edit", input);
        }
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDirector(int id)
    {
        await _directors.DeleteAsync(id);

        return RedirectToAction(nameof(Directors));
    }

    public async Task<IActionResult> Curricula()
    {
        return View("Curricula", await _curricula.GetAllAsync());
    }

    [HttpGet]
    public async Task<IActionResult> AddCurriculum()
    {
        await LoadCurriculumLists();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCurriculum(
        string term,
        int schoolId,
        int classId,
        List<int> subjectIds,
        List<int> teacherIds,
        List<string> days,
        List<int> periods)
    {
        subjectIds ??= new List<int>();
        teacherIds ??= new List<int>();
        days ??= new List<string>();
        periods ??= new List<int>();

        var entries = new List<CreateCurriculumEntryDto>();

        for (int i = 0; i < subjectIds.Count; i++)
        {
            var subjectId = subjectIds.ElementAtOrDefault(i);
            var teacherId = teacherIds.ElementAtOrDefault(i);

            if (subjectId <= 0 || teacherId <= 0)
                continue;

            entries.Add(new CreateCurriculumEntryDto(
                subjectId,
                teacherId,
                days.ElementAtOrDefault(i) ?? "",
                periods.ElementAtOrDefault(i)
            ));
        }

        if (string.IsNullOrWhiteSpace(term) || schoolId <= 0 || classId <= 0 || entries.Count == 0)
        {
            ModelState.AddModelError("", "Invalid curriculum data.");
            await LoadCurriculumLists(schoolId);
            return View();
        }

        var exists = await _db.Curricula
            .AnyAsync(c => c.ClassId == classId && c.Term == term);

        if (exists)
        {
            ModelState.AddModelError("", "This class already has a curriculum for this term.");
            await LoadCurriculumLists(schoolId);
            return View();
        }

        await _curricula.CreateAsync(new CreateCurriculumDto(term, classId, entries));

        return RedirectToAction(nameof(Curricula));
    }

    [HttpGet]
    public async Task<IActionResult> EditCurriculum(int id)
    {
        var curriculum = await _db.Curricula
            .Include(c => c.Class)
            .Include(c => c.Entries)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curriculum == null)
            return NotFound();

        await LoadCurriculumLists(curriculum.Class?.SchoolId);

        return View("EditCurriculum", curriculum);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCurriculum(
        int id,
        string term,
        int schoolId,
        int classId,
        List<int> subjectIds,
        List<int> teacherIds,
        List<string> days,
        List<int> periods)
    {
        var curriculum = await _db.Curricula
            .Include(c => c.Entries)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (curriculum == null)
            return NotFound();

        subjectIds ??= new List<int>();
        teacherIds ??= new List<int>();
        days ??= new List<string>();
        periods ??= new List<int>();

        var exists = await _db.Curricula
            .AnyAsync(c => c.Id != id &&
                           c.ClassId == classId &&
                           c.Term == term);

        if (exists)
        {
            ModelState.AddModelError("", "This class already has a curriculum for this term.");
            await LoadCurriculumLists(schoolId);
            return View("EditCurriculum", curriculum);
        }

        curriculum.Term = term;
        curriculum.ClassId = classId;

        _db.CurriculumEntries.RemoveRange(curriculum.Entries);

        for (int i = 0; i < subjectIds.Count; i++)
        {
            var subjectId = subjectIds.ElementAtOrDefault(i);
            var teacherId = teacherIds.ElementAtOrDefault(i);

            if (subjectId <= 0 || teacherId <= 0)
                continue;

            curriculum.Entries.Add(new CurriculumEntry
            {
                SubjectId = subjectId,
                TeacherId = teacherId,
                DayOfWeek = days.ElementAtOrDefault(i) ?? "",
                Period = periods.ElementAtOrDefault(i)
            });
        }

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Curricula));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCurriculum(int id)
    {
        await _curricula.DeleteAsync(id);

        return RedirectToAction(nameof(Curricula));
    }

    public async Task<IActionResult> Statistics(int? schoolId)
    {
        ViewBag.Schools = new SelectList(await _schools.GetAllAsync(), "Id", "Name", schoolId);

        ViewBag.SubjectAverages = schoolId.HasValue
            ? await _stats.GetSubjectAveragesBySchoolAsync(schoolId.Value)
            : await _stats.GetGlobalSubjectAveragesAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return Redirect("/Identity/Account/Login");
    }

    private async Task<List<SelectListItem>> GetStudentSelectListAsync()
    {
        var students = await _db.Students.ToListAsync();

        var result = new List<SelectListItem>();

        foreach (var student in students)
        {
            var user = await _userManager.FindByIdAsync(student.UserId);

            result.Add(new SelectListItem
            {
                Value = student.Id.ToString(),
                Text = user != null
                    ? $"{user.FirstName} {user.LastName}"
                    : $"Student #{student.Id}"
            });
        }

        return result;
    }

    private async Task LoadTeacherLists(
        IEnumerable<int>? selectedSubjectIds = null,
        IEnumerable<int>? selectedClassIds = null,
        int? selectedSchoolId = null)
    {
        ViewBag.Schools = new SelectList(
            await _schools.GetAllAsync(),
            "Id",
            "Name",
            selectedSchoolId
        );

        ViewBag.Subjects = new MultiSelectList(
            await _subjects.GetAllAsync(),
            "Id",
            "Name",
            selectedSubjectIds
        );

        ViewBag.Classes = new MultiSelectList(
            await _db.Classes
                .OrderBy(c => c.Name)
                .ToListAsync(),
            "Id",
            "Name",
            selectedClassIds
        );
    }

    private async Task LoadStudents(IEnumerable<int>? selectedStudentIds = null)
    {
        ViewBag.Students = new MultiSelectList(
            await _students.GetAllAsync(),
            "Id",
            "FullName",
            selectedStudentIds
        );
    }

    private async Task LoadCurriculumLists(int? selectedSchoolId = null)
    {
        ViewBag.Schools = new SelectList(
            await _schools.GetAllAsync(),
            "Id",
            "Name",
            selectedSchoolId
        );

        ViewBag.Classes = new SelectList(
            await _db.Classes
                .Include(c => c.School)
                .Where(c => !selectedSchoolId.HasValue
                            || c.SchoolId == selectedSchoolId.Value)
                .OrderBy(c => c.Name)
                .ToListAsync(),
            "Id",
            "Name"
        );

        ViewBag.Subjects = await _subjects.GetAllAsync();
        ViewBag.Teachers = await _teachers.GetAllAsync();
    }

    public class TeacherRowVm
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public int SchoolId { get; set; }
        public List<string> SubjectNames { get; set; } = new();
        public List<string> ClassNames { get; set; } = new();
    }

    public class ParentsIndexVm
    {
        public List<ParentRowVm> Parents { get; set; } = new();
    }

    public class ParentRowVm
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> StudentNames { get; set; } = new();
    }

    private static List<int> ParseClassIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new List<int>();

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.TryParse(x, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct()
            .ToList();
    }
}