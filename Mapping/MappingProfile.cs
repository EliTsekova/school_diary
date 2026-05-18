using AutoMapper;
using school_diary.Dtos;
using school_diary.Models;

namespace school_diary.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateStudentDto, Student>()
                .ForMember(d => d.ClassId, cfg => cfg.Ignore())
                .ForMember(d => d.Class, cfg => cfg.Ignore())
                .ForMember(d => d.School, cfg => cfg.Ignore())
                .ForMember(d => d.User, cfg => cfg.MapFrom(s => new User
                {
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    UserName = s.Email,
                    Role = Role.Student
                }));

            CreateMap<UpdateStudentDto, Student>()
                .ForMember(d => d.User, cfg => cfg.Ignore())
                .ForMember(d => d.ClassId, cfg => cfg.Ignore())
                .ForMember(d => d.Class, cfg => cfg.Ignore())
                .ForMember(d => d.School, cfg => cfg.Ignore());

            CreateMap<Student, StudentDto>()
                .ForCtorParam("Id", opt => opt.MapFrom(s => s.Id))
                .ForCtorParam("FullName", opt => opt.MapFrom(s => s.User.FirstName + " " + s.User.LastName))
                .ForCtorParam("Email", opt => opt.MapFrom(s => s.User.Email!))
                .ForCtorParam("ClassName", opt => opt.MapFrom(s => s.Class != null ? s.Class.Name : null))
                .ForCtorParam("SchoolId", opt => opt.MapFrom(s => s.SchoolId));

            CreateMap<CreateSchoolDto, School>();
            CreateMap<UpdateSchoolDto, School>();
            CreateMap<School, SchoolDto>();

            CreateMap<CreateDirectorDto, Director>();
            CreateMap<Director, DirectorDto>()
                .ConstructUsing(d => new DirectorDto(d.Id, d.UserId, d.SchoolId));

            CreateMap<Subject, SubjectDto>();

            CreateMap<CreateTeacherDto, Teacher>()
                .ForMember(d => d.User, cfg => cfg.MapFrom(t => new User
                {
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Email = t.Email,
                    UserName = t.Email,
                    Role = Role.Teacher
                }));

            CreateMap<UpdateTeacherDto, Teacher>()
                .ForMember(d => d.User, cfg => cfg.Ignore());

            CreateMap<Teacher, TeacherDto>()
                .ForCtorParam("FullName", opt => opt.MapFrom(t => t.User.FirstName + " " + t.User.LastName))
                .ForCtorParam("Email", opt => opt.MapFrom(t => t.User.Email))
                .ForCtorParam("SubjectIds", opt => opt.MapFrom(t =>
                    t.TeacherSubjects.Select(ts => ts.SubjectId).ToList()))
                .ForCtorParam("ClassIds", opt => opt.MapFrom(t =>
                    new List<int>()));

            CreateMap<CreateParentDto, Parent>()
                .ForMember(d => d.User, cfg => cfg.MapFrom(p => new User
                {
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Email = p.Email,
                    UserName = p.Email,
                    Role = Role.Parent
                }));

            CreateMap<Parent, ParentDto>()
                .ConstructUsing(p => new ParentDto(
                    p.Id,
                    $"{p.User.FirstName} {p.User.LastName}",
                    p.User.Email!
                ));

            CreateMap<CreateGradeDto, Grade>()
                .ForMember(d => d.CreatedOn, cfg => cfg.MapFrom(_ => DateTime.UtcNow));

            CreateMap<UpdateGradeDto, Grade>();
            CreateMap<Grade, GradeDto>();

            CreateMap<CreateAbsenceDto, Absence>();
            CreateMap<UpdateAbsenceDto, Absence>();
            CreateMap<Absence, AbsenceDto>();

            CreateMap<Curriculum, CurriculumDto>()
                .ConstructUsing(c => new CurriculumDto(
                    c.Id,
                    c.Term,
                    c.Class.SchoolId,
                    c.ClassId,
                    c.Class.Name,
                    c.Entries.Select(e => new CurriculumEntryDto(
                        e.Id,
                        e.SubjectId,
                        e.Subject.Name,
                        e.TeacherId,
                        e.Teacher.User.FirstName + " " + e.Teacher.User.LastName,
                        e.DayOfWeek,
                        e.Period
                    )).ToList()
                ));

            CreateMap<CurriculumEntry, CurriculumEntryDto>()
                .ForCtorParam("SubjectName", opt => opt.MapFrom(src => src.Subject.Name))
                .ForCtorParam("TeacherName", opt => opt.MapFrom(src =>
                    $"{src.Teacher.User.FirstName} {src.Teacher.User.LastName}"))
                .ForCtorParam("DayOfWeek", opt => opt.MapFrom(src => src.DayOfWeek))
                .ForCtorParam("Period", opt => opt.MapFrom(src => src.Period));
        }
    }
}