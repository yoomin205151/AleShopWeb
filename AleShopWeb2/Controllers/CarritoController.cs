using AleShopWeb2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;



namespace AleShopWeb2.Controllers
{
    public class CarritoController : Controller
    {
        private ALESHOPWEBEntities9 db = new ALESHOPWEBEntities9();
        // GET: Carrito
        public ActionResult Carrito()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AgregarAlCarrito(int productoId)
        {
            // Verificar si el usuario ha iniciado sesión y si tiene un ID válido en la sesión.
            if (Session["usuario"] != null && Session["usuario"] is usuario user && user.id > 0)
            {
                // Obtener el producto correspondiente al productoId
                var producto = db.producto.Find(productoId);

                if (producto == null)
                {
                    return Json(new { success = false, message = "El producto no existe." });
                }

                // Verificar si el producto tiene stock disponible
                if (producto.stock <= 0)
                {
                    return Json(new { success = false, message = "El producto no está disponible en stock." });
                }

                // Obtener el carrito del usuario actual desde la base de datos
                var carritoExistente = db.carrito.FirstOrDefault(c => c.id_usuario == user.id && c.id_producto == productoId);

                if (carritoExistente == null)
                {
                    // Si el producto no existe en el carrito, lo agregamos como un nuevo registro.
                    var carritoNuevo = new carrito
                    {
                        id_producto = productoId,
                        cantidad = 1,
                        id_usuario = user.id,
                        preciounitario = producto.precio
                    };

                    db.carrito.Add(carritoNuevo);
                }
                else
                {
                    // Si el producto ya existe en el carrito, simplemente actualizamos la cantidad.
                    carritoExistente.cantidad++;
                }

                // Guardar los cambios en la base de datos.
                db.SaveChanges();

                return Json(new { success = true, message = "Producto agregado al carrito correctamente." });
            }
            else
            {
                return Json(new { success = false, message = "Debe iniciar sesión para agregar productos al carrito." });
            }
        }

        [HttpPost]
        public ActionResult EliminarProducto(int productoId)
        {
            // Verificar si el usuario ha iniciado sesión y si tiene un ID válido en la sesión.
            if (Session["usuario"] != null && Session["usuario"] is usuario user && user.id > 0)
            {
                // Obtener el producto del carrito correspondiente al usuario y productoId.
                var carritoItem = db.carrito.FirstOrDefault(c => c.id_usuario == user.id && c.id == productoId);
                if (carritoItem == null)
                {
                    // El producto no existe en el carrito, retornar un error.
                    return Json(new { success = false, message = "El producto no existe en el carrito." });
                }

                // Eliminar el producto del carrito.
                db.carrito.Remove(carritoItem);
                db.SaveChanges();

                // Retornar un mensaje de éxito.
                return Json(new { success = true, message = "Producto eliminado exitosamente del carrito." });
            }
            else
            {
                // El usuario no ha iniciado sesión, retornar un error.
                return Json(new { success = false, message = "Debe iniciar sesión para modificar el carrito." });
            }
        }

        [HttpPost]
        public ActionResult SumarCantidad(int productoId)
        {
            // Verificar si el usuario ha iniciado sesión y si tiene un ID válido en la sesión.
            if (Session["usuario"] != null && Session["usuario"] is usuario user && user.id > 0)
            {
                // Obtener el producto del carrito correspondiente al usuario y productoId.
                var carritoItem = db.carrito.FirstOrDefault(c => c.id_usuario == user.id && c.id == productoId);
                if (carritoItem == null)
                {
                    // El producto no existe en el carrito, retornar un error.
                    return Json(new { success = false, message = "El producto no existe en el carrito." });
                }

                // Sumar una unidad a la cantidad del producto en el carrito.
                carritoItem.cantidad++;
                db.SaveChanges();

                // Retornar un mensaje de éxito.
                return Json(new { success = true, message = "Cantidad sumada exitosamente." });
            }
            else
            {
                // El usuario no ha iniciado sesión, retornar un error.
                return Json(new { success = false, message = "Debe iniciar sesión para modificar el carrito." });
            }
        }

        [HttpPost]
        public ActionResult RestarCantidad(int productoId)
        {
            // Verificar si el usuario ha iniciado sesión y si tiene un ID válido en la sesión.
            if (Session["usuario"] != null && Session["usuario"] is usuario user && user.id > 0)
            {
                // Obtener el producto del carrito correspondiente al usuario y productoId.
                var carritoItem = db.carrito.FirstOrDefault(c => c.id_usuario == user.id && c.id == productoId);
                if (carritoItem == null)
                {
                    // El producto no existe en el carrito, retornar un error.
                    return Json(new { success = false, message = "El producto no existe en el carrito." });
                }

                if (carritoItem.cantidad > 1)
                {
                    // Restar una unidad a la cantidad del producto en el carrito.
                    carritoItem.cantidad--;
                    db.SaveChanges();
                }

                // Retornar un mensaje de éxito.
                return Json(new { success = true, message = "Cantidad restada exitosamente." });
            }
            else
            {
                // El usuario no ha iniciado sesión, retornar un error.
                return Json(new { success = false, message = "Debe iniciar sesión para modificar el carrito." });
            }
        }

        public JsonResult DetalleCarrito()
        {
            // Verificar si el usuario ha iniciado sesión y si tiene un ID válido en la sesión.
            if (Session["usuario"] != null && Session["usuario"] is usuario user && user.id > 0)
            {
                var detalleCarrito = db.carrito
                    .Where(c => c.id_usuario == user.id)
                    .Select(c => new
                    {
                        c.id,
                        producto = new
                        {
                            c.producto.id,
                            c.producto.img,
                            categoria =  c.producto.categoria.nombre,
                            c.producto.nombre
                        },
                        c.cantidad,
                        subtotal = c.cantidad * c.preciounitario
                    })
                    .ToList();

                return Json(new { data = detalleCarrito }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { data = new object[0] }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult RealizarPago()
        {

           
            // Verificar si el usuario ha iniciado sesión y si tiene un ID válido en la sesión.
            if (Session["usuario"] != null && Session["usuario"] is usuario user && user.id > 0)
            {
                // Obtener los productos del carrito del usuario actual
                var carrito = db.carrito.Where(c => c.id_usuario == user.id).ToList();

                if (carrito.Count == 0)
                {
                    TempData["ErrorMessage"] = "El carrito está vacío. Agregue productos antes de realizar el pago.";
                    return Json(new { success = false });
                }

                try
                {
                    // Crear la venta con la información requerida
                    var venta = new venta
                    {
                        fecha = DateTime.Now,
                        total = carrito.Sum(c => c.preciounitario * c.cantidad),
                        id_origen = 1,
                        id_usuario = user.id
                    };

                    // Guardar la venta en la base de datos
                    db.venta.Add(venta);
                    db.SaveChanges();

                    // Crear los detalles de la venta asociados a la venta creada anteriormente
                    foreach (var item in carrito)
                    {
                        var detalleVenta = new detalle_venta
                        {
                            id_venta = venta.id,
                            id_producto = item.id_producto,
                            cantidad = item.cantidad,
                            precio = item.preciounitario
                        };
                        db.detalle_venta.Add(detalleVenta);

                        // Restar la cantidad comprada al stock del producto
                        var producto = db.producto.Find(item.id_producto);
                        if (producto != null)
                        {
                            producto.stock -= item.cantidad;
                        }
                    }

                    // Vaciar el carrito del usuario
                    foreach (var item in carrito)
                    {
                        db.carrito.Remove(item);
                    }
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Compra realizada con éxito.";
                    return Json(new { success = true });
                }
                catch
                {
                    TempData["ErrorMessage"] = "Error al realizar el pago. Por favor, inténtelo nuevamente.";
                    return Json(new { success = false });
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Debe iniciar sesión para realizar el pago.";
                return Json(new { success = false });
            }
        }

        


    }
}