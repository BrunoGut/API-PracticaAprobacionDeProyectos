using Application.Dtos.Response;
using Application.Interfaces.IQuery;
using Application.Interfaces.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCase
{
    public class UserService : IUserService
    {
        private readonly IUserQuery _query;

        public UserService(IUserQuery query)
        {
            _query = query;
        }

        public async Task<List<Users>> GetAllAsync()
        {
            var users = await _query.GetAllWithRolesAsync();

            return users.Select(u => new Users
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = new GenericResponse
                {
                    Id = u.Role,
                    Name = u.ApproverRole.Name
                }
            }).ToList();
        }
    }
}
