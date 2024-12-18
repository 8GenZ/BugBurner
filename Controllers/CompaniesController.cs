﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugBurner.Data;
using BugBurner.Models;
using BugBurner.Services.Interfaces;
using BugBurner.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugBurner.Services;

namespace BugBurner.Controllers
{
    public class CompaniesController : BTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTFileService _bTFileService;
        private readonly IBTCompanyService _btCompanyService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _bTRolesService;

        public CompaniesController(ApplicationDbContext context, IBTFileService bTFileService, IBTCompanyService bTCompanyService, UserManager<BTUser> userManager, IBTRolesService bTRolesService)
        {
            _context = context;
            _bTFileService = bTFileService;
            _btCompanyService = bTCompanyService;
            _userManager = userManager;
            _bTRolesService = bTRolesService;
            
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
            Company company = await _btCompanyService.GetCompanyInfoAsync(_companyId);

            return View(company);
        }

        // GET: Companies/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            Company company = await _btCompanyService.GetCompanyInfoAsync(_companyId);

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFormFile")] Company company)
        {
            if (ModelState.IsValid)
            {
                //set image data if one has been selected
                if (company.ImageFormFile != null)
                {
                    company.ImageFileData = await _bTFileService.ConvertFileToByteArrayAsync(company.ImageFormFile);
                    company.ImageFileType = company.ImageFormFile.ContentType;
                }

                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageFormFile,ImageFileData,ImageFileType")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //set new image data if one has been selected
                    if (company.ImageFormFile != null)
                    {
                        company.ImageFileData = await _bTFileService.ConvertFileToByteArrayAsync(company.ImageFormFile);
                        company.ImageFileType = company.ImageFormFile.ContentType;
                    }

                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
          return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        [Authorize(Roles = "Admin")]
		[HttpGet]
		public async Task<IActionResult> ManageUserRoles()
		{
            // 1 - Add an instance of the ViewModel as a List (model)
            List<ManageUserRolesViewModel> model = new List<ManageUserRolesViewModel>();

			// 2 - Get CompanyId 

			// 3 - Get all company users
            List<BTUser> members = await _btCompanyService.GetMembersAsync(_companyId);
            // 4 - Loop over the users to populate the ViewModel
            //      - instantiate single ViewModel
            //      - use _rolesService
            //      - Create multiselect
            //      - viewmodel to model
            string? btUserId = _userManager.GetUserId(User);

            foreach(BTUser member in members)
            {
                if(string.Compare(btUserId, member.Id) != 0)
                {
                    ManageUserRolesViewModel viewModel = new();

                    IEnumerable<string>? currentRoles = await _bTRolesService.GetUserRolesAsync(member);

                    viewModel.BTUser = member;

                    viewModel.Roles = new MultiSelectList(await _bTRolesService.GetRolesAsync(), "Name", "Name", currentRoles);
                    model.Add(viewModel);
                }
            }

			// 5 - Return the model to the View
            return View(model);
		}

		[HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
		{
            // 1- Get the company Id

            // 2 - Instantiate the BTUser
            BTUser? bTUser = (await _btCompanyService.GetMembersAsync(_companyId)).FirstOrDefault(m => m.Id == viewModel.BTUser?.Id);
			// 3 - Get Roles for the User
			IEnumerable<string>? currentRoles = await _bTRolesService.GetUserRolesAsync(bTUser);
            // 4 - Get Selected Role(s) for the User
            string? selectedRole = viewModel.SelectedRoles?.FirstOrDefault();
            // 5 - Remove current role(s) and Add new role
            if (!string.IsNullOrEmpty(selectedRole))
            {
                if (await _bTRolesService.RemoveUserFromRolesAsync(bTUser, currentRoles))
                {
                    await _bTRolesService.AddUserToRoleAsync(bTUser, selectedRole);
                }
            }
			// 6 - Navigate
            return RedirectToAction(nameof(ManageUserRoles));
		}
	}
}
