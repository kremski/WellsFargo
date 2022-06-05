using AutoMapper;
using WellsFargo.Automapper;

namespace UnitTests.Automapper
{
    public class MappingTests
    {
        private readonly IMapper _mapper;

        public MappingTests() =>
            _mapper = new MapperConfiguration(cfg => { cfg.AddProfile<OmsOutputProfile>(); }).CreateMapper();

        [Fact]
        public void ValidateAutoMapperConfiguration() =>
            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
