﻿using CmsShop.Areas.Admin.Models.ViewModels.Shop;
using CmsShop.Models.Data;
using CmsShop.Models.ViewModels.Shop;
using PagedList;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CmsShop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            // list declaration
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                                    .ToArray()
                                    .OrderBy(x => x.Sorting)
                                    .Select(x => new CategoryVM(x)).ToList();
            }


            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName) 
        {
            // declaration id
            string id;

            using (Db db = new Db())
            {
                //validation if name of categories are unique
                if (db.Categories.Any(x => x.Name == catName))
                    return "tytulzajety";

                //  DTO init
                CategoryDTO dto = new CategoryDTO();
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 1000;

                db.Categories.Add(dto);
                db.SaveChanges();

                // Loading id
                id = dto.Id.ToString();
            }

            return id;
        }

        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public ActionResult ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                // counter init
                int count = 1;

                // DTO
                CategoryDTO dto;

                // categories sorting
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }

            return View();
        }

        // GET: Admin/Shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                // loading categories by id
                CategoryDTO dto = db.Categories.Find(id);

                // caregories removing
                db.Categories.Remove(dto);

                db.SaveChanges();
            }

            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //validation if category is unique
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "This categories already exists";

                // loading categories
                CategoryDTO dto = db.Categories.Find(id);

                // categories edition
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                db.SaveChanges();
            }

            return "Ok";
        }

        // GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //  model init
            ProductVM model = new ProductVM();

            // loading list of categories
            using (Db db =new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            // chacking model state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }              
            }

            // sprawdzenie czy nazwa produktu jest unikalna
            // validation if the name of product is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("","This product already exists!");
                    return View(model);
                }
            }

            // declaration product id
            int id;

            // adding product
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDto.Name;

                db.Products.Add(product);
                db.SaveChanges();

                // loading product id
                id = product.Id;
            }

            // message 
            TempData["SM"] = "Dodałeś produkt";

            #region Upload Image

            // catalogues structure configuration
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() );
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs" );
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery" );
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs" );

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);


            if (file != null && file.ContentLength > 0)
            {
                //extention validation
                string ext = file.ContentType.ToLower();
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The igage has not been uploaded - incorrect image extention.");
                        return View(model);
                    }                    
                }

                // image name init
                string imageName = file.FileName;

                // saving image
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                // save origin image
                file.SaveAs(path);

                // thrumbs saving
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }


            #endregion

            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            // product list declaration
            List<ProductVM> listOfProductVM;

            // page number set
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                // product list init
                listOfProductVM = db.Products.ToArray()
                                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                    .Select(x => new ProductVM(x))
                                    .ToList();

                // Category list for DeropDownList
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // set category
                ViewBag.SelectedCat = catId.ToString();
            }

            // pagging settings
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            //return list of product view model
            return View(listOfProductVM);
        }

        // GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            // productVM declaration
            ProductVM model;

            using (Db db = new Db())
            {
                // loading product for edition
                ProductDTO dto = db.Products.Find(id);

                // validation if product exist
                if (dto == null)
                {
                    return Content("This product does not exist.");
                }

                // model init
                model = new ProductVM(dto);

                
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                                .Select(fn => Path.GetFileName(fn));

            }


            return View(model);
        }

        // POST: Admin/Shop/EditProduct
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            
            int id = model.Id;

            
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }

            
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                            .Select(fn => Path.GetFileName(fn));

            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Ta nazwa produktu jest zajęta");
                    return View(model);
                }
            }

           
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDto = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDto.Name;

                db.SaveChanges();
            }

            
            TempData["SM"] = "Edytowałeś produkt";

            #region Image Upload

            
            if (file != null && file.ContentLength > 0)
            {
                
                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Obraz nie został przesłany - nieprawidłowe rozszerzenie obrazu.");
                        return View(model);
                    }
                }

                
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                    file2.Delete();

                foreach (var file3 in di2.GetFiles())
                    file3.Delete();

                
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                
                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion


            return RedirectToAction("EditProduct");
        }

        // GET: Admin/Shop/DeleteProduct/id
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }

            
            var orginalDirector = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            var pathString = Path.Combine(orginalDirector.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            return RedirectToAction("Products");
        }

        // POST: Admin/Shop/SaveGalleryImages/id
        [HttpPost]
        public ActionResult SaveGalleryImages(int id)
        {

           
            foreach (string fileName in Request.Files)
            {
                
                HttpPostedFileBase file = Request.Files[fileName];

                
                if (file != null && file.ContentLength > 0)
                {
                    
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);

                }

            }

            return View();
        }

        // POST: Admin/Shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id, string imageName)
        {
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }

        // GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            
            List<OrdersForAdminVM> ordersForAdminVM = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                foreach (var order in orders)
                {
                    
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.UserName;

                    foreach (var orderDetails in orderDetailsList)
                    {
                        
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        
                        decimal price = product.Price;

                        
                        string productName = product.Name;

                        
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        
                        total += orderDetails.Quantity * price;
                    }

                    ordersForAdminVM.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        Username = username,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            return View(ordersForAdminVM);
        }
    }
}