using AutoMapper;
using MathNet.Numerics.Distributions;
using Safepot.Entity;

namespace Safepot.Web.Api.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<SfpUser, SfpUserDto>();
        }
    }
}
