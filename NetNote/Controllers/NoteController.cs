using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetNote.Repository;
using NetNote.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NetNote.Controllers
{
    public class NoteController : Controller
    {
        private INoteRepository _notePrository;
        private INoteTypeRepository _noteTypeRepository;

        public NoteController(INoteRepository noteRepository, INoteTypeRepository noteTypeRepository)
        {
            _notePrository = noteRepository;
            _noteTypeRepository = noteTypeRepository;
        }

        //public async Task<IActionResult> Index()
        //{
        //    var notes =  await _notePrository.ListAsync();
        //    return View(notes);
        //}

        public IActionResult Index(int pageindex = 1)
        {
            var pagesize = 2;
            var notes = _notePrository.PageList(pageindex, pagesize);
            ViewBag.PageCount = notes.Item2;
            ViewBag.PageIndex = pageindex;
            return View(notes.Item1);
        }

        public async Task<ActionResult> Add()
        {
            var types = await _noteTypeRepository.ListAsync();
            ViewBag.Types = types.Select(a => new SelectListItem()
            {
                Text = a.Name,
                Value = a.Id.ToString()
            });
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromServices] IWebHostEnvironment env, NoteModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string filename = string.Empty;
            if (model.Attachment != null)
            {
                filename = Path.Combine("file", Guid.NewGuid().ToString() + Path.GetExtension(model.Attachment.FileName));
                using (var stream = new FileStream(Path.Combine(env.WebRootPath, filename), FileMode.CreateNew))
                {
                    model.Attachment.CopyTo(stream);
                }
            }

            await _notePrository.AddAsync(new Models.Note()
            {
                Title = model.Title,
                Content = model.Content,
                TypeId = model.Type,
                Create = DateTime.Now,
                Password = model.Password,
                Attachment = filename
            });
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detail(int id)
        {
            var note = await _notePrository.GetByIdAsync(id);
            if (!string.IsNullOrEmpty(note.Password))
            {
                return View();
            }
            return View(note);
        }

        [HttpPost]
        public async Task<IActionResult> Detail(int id,string password)
        {
            var note = await _notePrository.GetByIdAsync(id);
            if (!note.Password.Equals(password))
            {
                return BadRequest("密码错误，返回重新输入");
            }
            return View(note);
        }
    }
}
