using AutoMapper;
using Quiz.DataAccess.Models;
using Quiz.Shared.Requests;
using Quiz.Shared.Responses;

namespace Quiz.API.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, RegisterResponseDto>(MemberList.Destination);
            CreateMap<Question, QuestionResponseDto>(MemberList.Destination);
            CreateMap<DataAccess.Models.Quiz, QuizResponseDto>(MemberList.Destination)
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User.UserName));
            CreateMap<DataAccess.Models.Quiz, JoinResponseDto>(MemberList.Destination)
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.User.UserName));
            CreateMap<Question, ExtendedQuestionResponseDto>(MemberList.Destination);
            CreateMap<DataAccess.Models.Quiz, ExtendedQuizResponseDto>(MemberList.Destination)
                .ForMember(dest => dest.Pin, opt => opt.MapFrom(src => src.Pin.HasValue ? src.Pin.Value.ToString("D6") : null));
            CreateMap<User, UserResponseDto>(MemberList.Destination);

            CreateMap<RegisterRequestDto, User>(MemberList.Source)
                .ForSourceMember(src => src.Password, opt => opt.DoNotValidate());
            CreateMap<QuestionRequestDto, Question>(MemberList.Source);
            CreateMap<QuizRequestDto, DataAccess.Models.Quiz>(MemberList.Source)
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}
