using Api.Dtos.Dependent;
using Api.Interfaces.Repositories;
using Api.Interfaces.Services;
using Api.Models;
using Api.Utilities;

using AutoMapper;

namespace Api.Services
{
    public class DependentService : IDependentService
    {
        private readonly IDependentRepository _dependentRepository;
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;
        

        public DependentService(IDependentRepository dependenetRepository, IEmployeeService employeeService, IMapper mapper)
        {
            _dependentRepository = dependenetRepository;
            _employeeService = employeeService;
            _mapper = mapper;
        }

        public async Task<ApiResponse<AddDependentDto>> Add(AddDependentDto dependentDto, int employeeId)
        {
            var result = new ApiResponse<AddDependentDto>();
            var dependentModel = _mapper.Map<Dependent>(dependentDto); 
            var employeeDto = await _employeeService.Get(employeeId);

            
            // Make sure employee has 1 spouse or 1 domestic partner at most
            var partnerRelationships = new[] { Relationship.Spouse, Relationship.DomesticPartner };

            if (partnerRelationships.Contains(dependentModel.Relationship))
            {
                // Count existing partners
                var existingPartnerCount = employeeDto.Dependents
                    .Count(d => partnerRelationships.Contains(d.Relationship));

                if (existingPartnerCount == 1)
                {
                    result = ApiResponseUtil.CreateResponse<AddDependentDto>(false, null, "Cannot have more than one spouse or domestic partner.", "INFO-KEY");
                    return result;
                }
            }

            dependentModel.EmployeeId = employeeId;

            try
            {
                var addedDependentModel = await _dependentRepository.Add(dependentModel);
                var newDependentDto = _mapper.Map<AddDependentDto>(addedDependentModel);

                result = ApiResponseUtil.CreateResponse(true, newDependentDto, "New Dependent Created.");
                return result;
            }
            catch (Exception ex)
            {
                /* 
                 * My idea here is we want to catch db exceptions like failed connections, etc and log them/report them to devs, but let the api respond with a 500 so that
                 * api can continue running. We catch here and not repo layer, so that service can generate a user-digestable response and notify dev team with logs
                */

                // Log confidential exception message to logging server
                // return user-friendly error

                result = ApiResponseUtil.CreateResponse<AddDependentDto>(false, null, "Sorry, something went wrong.", "ERROR-KEY");
                return result;
            }
        }

        public Task<ApiResponse<GetDependentDto>> Get(int dependentId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<ICollection<GetDependentDto>>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<ICollection<GetDependentDto>>> GetAllDependentsByEmployee(int employeeId)
        {
            throw new NotImplementedException();
        }
    }
}
