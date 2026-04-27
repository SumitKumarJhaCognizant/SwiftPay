using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SwiftPay.Repositories.Interfaces;
using SwiftPay.Configuration;
using SwiftPay.Models;

namespace SwiftPay.Repositories

{

	public class RoleRepository : IRoleRepository
	{
		private readonly AppDbContext _db;


		public RoleRepository(AppDbContext db)

		{
			_db = db;

		}

		public async Task<Role> CreateAsync(Role entity)

		{
			await _db.Set<Role>().AddAsync(entity);

			await _db.SaveChangesAsync();

			return entity;

		}

		public async Task<Role> GetByIdAsync(int roleId)

		{
			return await _db.Set<Role>()

				.Include(r => r.UserRoles)

				.FirstOrDefaultAsync(r => r.RoleId == roleId && !r.IsDeleted);

		}

		public async Task<Role> GetByRoleTypeAsync(SwiftPay.Constants.Enums.RoleType roleType)

		{
			return await _db.Set<Role>()

				.Include(r => r.UserRoles)

				.FirstOrDefaultAsync(r => r.RoleType == roleType && !r.IsDeleted);

		}

		public async Task<IEnumerable<Role>> GetAllAsync()

		{
			return await _db.Set<Role>()

				.Include(r => r.UserRoles)

				.Where(r => !r.IsDeleted)

				.ToListAsync();

		}

		public async Task<Role> UpdateAsync(Role entity)

		{
			_db.Set<Role>().Update(entity);

			await _db.SaveChangesAsync();

			return entity;

		}


		public async Task<bool> DeleteAsync(int roleId)

		{

			var role = await GetByIdAsync(roleId);

			if (role == null)

				return false;


			role.IsDeleted = true;

			role.UpdatedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync();

			return true;

		}

	}

}