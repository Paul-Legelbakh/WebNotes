﻿using AutoMapper;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebNotes.Crypt;
using WebNotesDataBase.DAL;
using WebNotesDataBase.Models;
using WebNotesDataBase.ViewModels;

namespace WebNotes.Controllers
{
    public class NotesController : Controller
    {
        private UserRepository uowUser;
        private NoteRepository uowNote;
        public GenericRepository<Note> noteRepository;
        public GenericRepository<User> userRepository;

        // GET Notes | Create new connections of database
        public NotesController()
        {
            uowUser = new UserRepository();
            uowNote = new NoteRepository();
            noteRepository = uowNote.unitOfWork.EntityRepository;
            userRepository = uowUser.unitOfWork.EntityRepository;
        }

        public ActionResult NoteSearch(int? page, string searchCondition)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            if (Request.Cookies["login"] != null)
            {
                User usr = userRepository.GetByID(Convert.ToInt64(Request.Cookies["login"].Value));
                var notes = Mapper.Map<IEnumerable<Note>, List<IndexNoteViewModel>>(uowNote.GetListByUserID(Convert.ToInt32(usr.UserId)));
                foreach (IndexNoteViewModel note in notes)
                {
                    note.Label = EncryptDecrypt.DecryptData(note.Label, usr.Pass);
                    note.NameAuthor = EncryptDecrypt.DecryptData(note.NameAuthor, usr.Pass);
                }
                var decNotes = notes.Where(a => a.Label.Contains(searchCondition)).ToList();
                if (decNotes.Count <= 0)
                {
                    ViewBag.NoteNotFound = "Notes with this value is not found!";
                    return View("Index", notes.ToPagedList(pageNumber, pageSize));
                }
                if (searchCondition == null) return RedirectToAction("Index");
                return View("Index", decNotes.ToPagedList(pageNumber, pageSize));
            }
            else return RedirectToAction("../Users/Login");
        }

        public ActionResult Index(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            if (Request.Cookies["login"] != null)
            {
                User usr = userRepository.GetByID(Convert.ToInt64(Request.Cookies["login"].Value));
                var notes = Mapper.Map<IEnumerable<Note>, List<IndexNoteViewModel>>(uowNote.GetListByUserID(Convert.ToInt32(usr.UserId)));
                foreach (IndexNoteViewModel note in notes)
                {
                    note.Label = EncryptDecrypt.DecryptData(note.Label, usr.Pass);
                    note.NameAuthor = EncryptDecrypt.DecryptData(note.NameAuthor, usr.Pass);
                }
                return View(notes.ToPagedList(pageNumber, pageSize));
            }
            else return RedirectToAction("../Users/Login");
        }

        // GET Notes Details
        public ActionResult Details(int? id)
        {
            IndexNoteViewModel note = null;
            if (Request.Cookies["login"] != null)
            {
                User usr = userRepository.GetByID(Convert.ToInt64(Request.Cookies["login"].Value));
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                note = Mapper.Map<Note, IndexNoteViewModel>(noteRepository.GetByID(id));
                note.Label = EncryptDecrypt.DecryptData(note.Label, usr.Pass);
                note.Body = EncryptDecrypt.DecryptData(note.Body, usr.Pass);
                note.NameAuthor = EncryptDecrypt.DecryptData(note.NameAuthor, usr.Pass);
                if (note == null)
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ViewBag.ErrorLogin = "You are not autorized!";
                return View("../Users/Login");
            }
            return View(note);
        }

        // GET Notes Create
        public ActionResult Create()
        {
            return View();
        }

        // POST Notes Create
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateNoteViewModel model)
        {
            if (Request.Cookies["login"] != null)
            {
                User usr = userRepository.GetByID(Convert.ToInt64(Request.Cookies["login"].Value));
                if (ModelState.IsValid)
                {
                    var note = Mapper.Map<CreateNoteViewModel, Note>(model);
                    note.CreatedDate = DateTime.Now;
                    note.EditedDate = DateTime.Now;
                    note.UserId = usr.UserId;
                    note.Label = EncryptDecrypt.EncryptData(note.Label, usr.Pass);
                    note.Body = EncryptDecrypt.EncryptData(note.Body, usr.Pass);
                    noteRepository.Insert(note);
                    noteRepository.Save();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                ViewBag.ErrorLogin = "You are not autorized!";
                return View("../Users/Login");
            }
            return View(model);
        }

        // GET Notes Edit
        public ActionResult Edit(int? id)
        {
            CreateNoteViewModel note = null;
            if (Request.Cookies["login"] != null)
            {
                User usr = userRepository.GetByID(Convert.ToInt64(Request.Cookies["login"].Value));
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                note = Mapper.Map<Note, CreateNoteViewModel>(noteRepository.GetByID(id));
                if (note == null)
                {
                    return HttpNotFound();
                }
                note.Label = EncryptDecrypt.DecryptData(note.Label, usr.Pass);
                note.Body = EncryptDecrypt.DecryptData(note.Body, usr.Pass);
            }
            else
            {
                ViewBag.ErrorLogin = "You are not authorized!";
                return View("../Users/Login");
            }
            return View(note);
        }

        // POST Notes Edit
        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CreateNoteViewModel model)
        {
            if (Request.Cookies["login"] != null)
            {
                User usr = userRepository.GetByID(Convert.ToInt64(Request.Cookies["login"].Value));
                if (ModelState.IsValid)
                {
                    Note note = Mapper.Map<CreateNoteViewModel, Note>(model);
                    Note nt = noteRepository.GetByID(note.NoteId);
                    nt.EditedDate = DateTime.Now;
                    nt.Label = EncryptDecrypt.EncryptData(note.Label, usr.Pass);
                    nt.Body = EncryptDecrypt.EncryptData(note.Body, usr.Pass);
                    noteRepository.Update(nt);
                    noteRepository.Save();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                ViewBag.ErrorLogin = "You are not autorized!";
                return View("../Users/Login");
            }
            return View(model);
        }

        // GET Notes Delete
        public ActionResult Delete(int? id)
        {
            IndexNoteViewModel note = null;
            if (Request.Cookies["login"] != null)
            {
                User usr = userRepository.GetByID(Convert.ToInt64(Request.Cookies["login"].Value));
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                note = Mapper.Map<Note, IndexNoteViewModel>(noteRepository.GetByID(id));
                note.Label = EncryptDecrypt.DecryptData(note.Label, usr.Pass);
                note.Body = EncryptDecrypt.DecryptData(note.Body, usr.Pass);
                note.NameAuthor = EncryptDecrypt.DecryptData(note.NameAuthor, usr.Pass);
                if (note == null)
                {
                    return HttpNotFound();
                }
            }
            else
            {
                ViewBag.ErrorLogin = "You are not autorized!";
                return View("../Users/Login");
            }
            return View(note);
        }

        // POST Notes Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            noteRepository.Delete(id);
            noteRepository.Save();
            return RedirectToAction("Index");
        }
    }
}
