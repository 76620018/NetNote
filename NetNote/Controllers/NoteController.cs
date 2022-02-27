using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetNote.Repository;
using NetNote.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetNote.Controllers
{
    public class NoteController : Controller
    {
        private INoteRepository _notePrository;
        private INoteTypeRepository _noteTypeRepository;

        public NoteController(INoteRepository noteRepository,INoteTypeRepository noteTypeRepository)
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
            var pagesize = 10;
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
        public async Task<IActionResult> Add(NoteModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _notePrository.AddAsync(new Models.Note()
            {
                Title = model.Title,
                Content = model.Content,
                TypeId= model.Type,
                Create = DateTime.Now
            });
            return RedirectToAction("Index");
        }
    }
}
