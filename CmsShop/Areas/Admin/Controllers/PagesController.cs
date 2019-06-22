using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //PageVM Declaration
            List<PageVM> pagesList;

           
            using (Db db = new Db())
            {
                // List init
                pagesList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }


            // return pades to view
            return View(pagesList);
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {

            // model state checking
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                string slug;

                // PageDTO init
                PageDTO dto = new PageDTO();
     
                // when slug not exist assign title
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                // title page validation
                if (db.Pages.Any(x => x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "This title already exists.");
                    return View(model);
                }

                dto.Title = model.Title;
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 1000;

                // saving DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["SM"] = "New page has been added.";

            return RedirectToAction("AddPage");
        }

        // GET: Admin/Pages/EditPage
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            // PageVM declaration
            PageVM model;

            using (Db db = new Db())
            {
                // loading page from db
                PageDTO dto = db.Pages.Find(id);

                // checking if page exists
                if (dto == null)
                {
                    return Content("Thepage does not exists!");
                }

                model = new PageVM(dto);
            }


            return View(model);
        }

        // POST: Admin/Pages/EditPage
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                // loading page id
                int id = model.Id;

                // slug loading
                string slug = "home";

                // loading page for edit
                PageDTO dto = db.Pages.Find(id);

                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //unique page validation
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Page or adderess aleready exist.");
                }

                // DTO modification
                dto.Title = model.Title;
                dto.Slug = slug;
                dto.HasSidebar = model.HasSidebar;
                dto.Body = model.Body;

                //siving edited page
                db.SaveChanges();
            }

            // message 
            TempData["SM"] = "The page has been edited successful";

            //Redirect
            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/Details/id
        [HttpGet]
        public ActionResult Details(int id)
        {
            
            PageVM model;

            using (Db db = new Db())
            {
                
                PageDTO dto = db.Pages.Find(id);

                
                if (dto == null)
                {
                    return Content("Strona o podanym id nie istnieje.");
                }

                model = new PageVM(dto);
            }

            return View(model);
        }

        // GET: Admin/Pages/Delete/id
        [HttpGet]
        public ActionResult Delete(int id)
        {
            using (Db db = new Db())
            {              
                PageDTO dto = db.Pages.Find(id);
                                
                db.Pages.Remove(dto);
                                
                db.SaveChanges();
            }

            // Redirect
            return RedirectToAction("Index");
        }

        // POST: Admin/Pages/ReorderPages
        [HttpPost]
        public ActionResult ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                int count = 1;
                PageDTO dto;

                // sorting
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }
            }

            return View();
        }

        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            // SidebarVM declaration
            SidebarVM model;

            using (Db db = new Db())
            {
                // loading SidebarDTO
                SidebarDTO dto = db.Sidebar.Find(1);

                // model init
                model = new SidebarVM(dto);
            }

            return View(model);
        }

        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                // loading Sidebar DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                // modification Sidebar
                dto.Body = model.Body;

                
                db.SaveChanges();
            }

            //message
            TempData["SM"] = "Sidebar has been modified";

            // Redirect
            return RedirectToAction("EditSidebar");
        }
    }
}