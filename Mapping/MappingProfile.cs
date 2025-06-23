using AutoMapper;
using school_diary.Models;
using school_diary.Dtos;

namespace school_diary.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateStudentDto, Student>()
                .ForMember(d => d.User, cfg => cfg.MapFrom(s => new User
                {
                    FirstName = s.FirstName,
                    LastName  = s.LastName,
                    Email     = s.Email,
                    Role      = Role.Student
                }));

            CreateMap<UpdateStudentDto, Student>()
                .ForMember(d => d.User, cfg => cfg.Ignore());

            CreateMap<Student, StudentDto>()
                .ConstructUsing(s => new StudentDto(
                    s.Id,
                    $"{s.User.FirstName} {s.User.LastName}",
                    s.User.Email,
                    s.ClassName,
                    s.SchoolId
                ));
        }
    }
}