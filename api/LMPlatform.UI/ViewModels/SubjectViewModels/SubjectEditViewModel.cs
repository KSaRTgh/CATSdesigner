﻿using Application.Core;
using Application.Infrastructure.CPManagement;
using Application.Infrastructure.FoldersManagement;
using Application.Infrastructure.GroupManagement;
using Application.Infrastructure.KnowledgeTestsManagement;
using Application.Infrastructure.MaterialsManagement;
using Application.Infrastructure.StudentManagement;
using Application.Infrastructure.SubjectManagement;
using LMPlatform.Data.Infrastructure;
using LMPlatform.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace LMPlatform.UI.ViewModels.SubjectViewModels
{
    using Application.Core.Data;

    public class SubjectEditViewModel
    {
        private readonly LazyDependency<IModulesManagementService> _modulesManagementService = new LazyDependency<IModulesManagementService>();
        private readonly LazyDependency<ISubjectManagementService> _subjectManagementService = new LazyDependency<ISubjectManagementService>();
		private readonly LazyDependency<IGroupManagementService> _groupManagementService = new LazyDependency<IGroupManagementService>();
        private readonly LazyDependency<IFoldersManagementService> _foldersManagementService = new LazyDependency<IFoldersManagementService>();
        private readonly LazyDependency<IMaterialsManagementService> _materialsManagementService = new LazyDependency<IMaterialsManagementService>();
        private readonly LazyDependency<ICpContext> context = new LazyDependency<ICpContext>();
        private readonly LazyDependency<ICPManagementService> _cpManagementService = new LazyDependency<ICPManagementService>();

		private readonly LazyDependency<IStudentManagementService> _studentManagementService = new LazyDependency<IStudentManagementService>();

		private IStudentManagementService StudentManagementService
		{
			get { return _studentManagementService.Value; }
		}

		private readonly LazyDependency<ITestsManagementService> _testsManagementService = new LazyDependency<ITestsManagementService>();

		private ITestsManagementService TestsManagementService
		{
			get { return _testsManagementService.Value; }
		}

        private ICPManagementService CPManagementService
        {
            get { return _cpManagementService.Value; }
        }

        private ICpContext Context
        {
            get { return context.Value; }
        }

        private IGroupManagementService GroupManagementService
		{
			get
			{
				return _groupManagementService.Value;
			}
		}

        private ISubjectManagementService SubjectManagementService
        {
            get
            {
                return _subjectManagementService.Value;
            }
        }

        private IModulesManagementService ModulesManagementService
        {
            get
            {
                return _modulesManagementService.Value;
            }
        }

        private IFoldersManagementService FoldersManagementService
        {
            get
            {
                return _foldersManagementService.Value;
            }
        }

        private IMaterialsManagementService MaterialsManagementService
        {
            get
            {
                return _materialsManagementService.Value;
            }
        }

        public ICollection<ModulesViewModel> Modules
        {
            get; 
            set;
        }

        [Required(ErrorMessage = "Название предмета не может быть пустым")]
		public string DisplayName
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Аббревиатура не может быть пустой")]
        public string ShortName
        {
            get;
            set;
        }

        public string Title
        {
            get; 
            set;
        }

		public List<SelectListItem> Groups
		{
			get;
			set;
		}

        public int SubjectId
        {
            get;
            set;
        }

        public string CreatedBy { get; set; }

		public string Color { get; set; }

		public List<int> SelectedGroups
		{
			get;
			set;
		}

        public SubjectEditViewModel()
        {
            Modules = new Collection<ModulesViewModel>();
        }

        public SubjectEditViewModel(int subjectId)
        {
            SubjectId = subjectId;
			this.Color = "#ffffff";
            CreatedBy = SubjectManagementService.GetSubjectOwner(subjectId)?.FullName;
            Title = SubjectId == 0 ? "Создание предмета" : "Редактирование предмета";
            Modules = ModulesManagementService
                .GetModules().Where(e => e.Visible && e.ModuleType != ModuleType.SubjectAttachments).Select(e => new ModulesViewModel(e, e.Required))
                .OrderBy(x => x.Order)
                .ToList();
	        FillSubjectGroups();
            if (subjectId != 0)
            {
                var subject = SubjectManagementService.GetSubject(subjectId);
				this.Color = subject.Color;
                SubjectId = subjectId;
                ShortName = subject.ShortName;
                DisplayName = subject.Name;
                foreach (var module in Modules)
                {
                    if (subject.SubjectModules.Any(e => e.ModuleId == module.ModuleId))
                    {
                        module.Checked = true;
                    }

                }

				SelectedGroups = new List<int>();
	            foreach (var group in Groups)
	            {
		            var groupId = int.Parse(group.Value);
		            if (subject.SubjectGroups.Any(e => e.GroupId == groupId && e.IsActiveOnCurrentGroup))
		            {
			            SelectedGroups.Add(groupId);
		            }
	            }
            }
        }

	    private void FillSubjectGroups()
	    {
		    var groups = GroupManagementService.GetGroups();
			Groups = groups.Select(e => new SelectListItem
			{
				Text = e.Name,
				Value = e.Id.ToString(CultureInfo.InvariantCulture),
				Selected = false
			}).ToList();
	    }

        public void Save(int userId, string color)
        {		
            var subject = new Subject
            {
                Id = SubjectId,
                Name = DisplayName,
                ShortName = ShortName,
				Color = color,
                SubjectModules = new Collection<SubjectModule>(),
                SubjectLecturers = new Collection<SubjectLecturer>()
            };

            foreach (var module in Modules)
            {
                if (module.Checked)
                {
                    if (module.Type == ModuleType.Labs)
                    {
                        subject.SubjectModules.Add(new SubjectModule
                        {
                            ModuleId = ModulesManagementService.GetModules().First(e => e.ModuleType == ModuleType.ScheduleProtection).Id,
                            SubjectId = SubjectId
                        });
                        subject.SubjectModules.Add(new SubjectModule
                        {
                            ModuleId = ModulesManagementService.GetModules().First(e => e.ModuleType == ModuleType.StatisticsVisits).Id,
                            SubjectId = SubjectId
                        });
                        subject.SubjectModules.Add(new SubjectModule
                        {
                            ModuleId = ModulesManagementService.GetModules().First(e => e.ModuleType == ModuleType.Results).Id,
                            SubjectId = SubjectId
                        });
                    }

                    subject.SubjectModules.Add(new SubjectModule
                    {
                        ModuleId = module.ModuleId,
                        SubjectId = SubjectId
                    });
                }
            }

            var attachmentsModule = ModulesManagementService.GetModule(new Query<Module>(x => x.ModuleType == ModuleType.SubjectAttachments));

            if (!subject.SubjectModules.Any(x => x.ModuleId == attachmentsModule.Id) && Modules.Where(x => x.Checked)
                .Any(x => x.Type == ModuleType.Labs || x.Type == ModuleType.Practical || x.Type == ModuleType.Lectures))
            {
                subject.SubjectModules.Add(new SubjectModule
                {
                    ModuleId = attachmentsModule.Id,
                    SubjectId = SubjectId
                });
            }

            subject.SubjectLecturers.Add(new SubjectLecturer
            {
                SubjectId = SubjectId,
                LecturerId = userId,
                Owner = SubjectId == 0 ? userId : SubjectManagementService.GetSubjectOwner(SubjectId)?.Id ?? userId
            });

			var selectedGroupdsOld = new List<SubjectGroup>();

			if (this.SubjectId != 0)
			{
				selectedGroupdsOld = this.SubjectManagementService.GetSubject(new Query<Subject>(e => e.Id == this.SubjectId).Include(e => e.SubjectGroups)).SubjectGroups.ToList();
			}

            var oldGroupIds = selectedGroupdsOld.Select(x => x.GroupId);            

	        if (SelectedGroups != null)
	        {
				UpdateNewAssignedGroups(Modules.Where(e => e.Checked == true), SelectedGroups.Where(e => !oldGroupIds.Contains(e)).ToList());
				subject.SubjectGroups = SelectedGroups.Select(e => new SubjectGroup
				{
					GroupId = e,
					SubjectId = SubjectId,
				    IsActiveOnCurrentGroup = true
                }).ToList();    
	        }
	        else
	        {
				subject.SubjectGroups = new Collection<SubjectGroup>();    
	        }

			foreach (var subjectSubjectGroup in selectedGroupdsOld)
			{
				if (!subject.SubjectGroups.Any(e => e.GroupId == subjectSubjectGroup.GroupId))
				{
					this.TestsManagementService.UnlockAllTestForGroup(subjectSubjectGroup.GroupId);
                    this.SubjectManagementService.DeleteNonReceivedUserFiles(subjectSubjectGroup.GroupId, subject.Id);
				}
			}

            var acp = Context.AssignedCourseProjects.Include("Student").Where(x => x.CourseProject.SubjectId == subject.Id);

            foreach (var a in acp)
            {
                bool flag = false;
                foreach(var s in subject.SubjectGroups)
                {
                    if (s.GroupId == a.Student.GroupId)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    Context.AssignedCourseProjects.Remove(a);
                }
                CPManagementService.DeletePercentageAndVisitStatsForUser(a.StudentId);
            }

            var cpg = Context.CourseProjectGroups.Where(x => x.CourseProject.SubjectId == subject.Id);

            foreach (var a in cpg)
            {
                bool flag = false;
                foreach (var s in subject.SubjectGroups)
                {
                    if (s.GroupId == a.GroupId)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    Context.CourseProjectGroups.Remove(a);
                }
            }

            Context.SaveChanges();

            Subject sub = SubjectManagementService.SaveSubject(subject);
	        this.SubjectId = sub.Id;
	        this.CreateOrUpdateSubGroups();

            foreach (var module in sub.SubjectModules)
            {
                if (module.ModuleId == 9)
                {
                    MaterialsManagementService.CreateRootFolder(module.Id, sub.Name);
                }
            } 
        }

	    private void CreateOrUpdateSubGroups()
	    {
		    var subject = this.SubjectManagementService.GetSubject(new Query<Subject>(e=> e.Id == this.SubjectId).Include(e => e.SubjectGroups.Select(x => x.SubGroups)));
			foreach (var subjectGroup in subject.SubjectGroups)
		    {
			    if (!subjectGroup.SubGroups.Any())
			    {
					var students = this.StudentManagementService.GetGroupStudents(subjectGroup.GroupId).Where(e => e.Confirmed == null || e.Confirmed.Value);
					this.SubjectManagementService.SaveSubGroup(this.SubjectId, subjectGroup.GroupId, students.Select(e => e.Id).ToList(), new List<int>(), new List<int>());
			    }
		    }
	    }

        private void UpdateNewAssignedGroups(IEnumerable<ModulesViewModel> modules, List<int> groupIds)
        {
            if(modules.Any(e => e.Type == ModuleType.YeManagment))
            {
                CPManagementService.SetSelectedGroupsToCourseProjects(SubjectId, groupIds);
            }
        }
    }
}
