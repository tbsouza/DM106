using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ThiagoStore.br.com.correios.ws;
using ThiagoStore.CRMClient;
using ThiagoStore.Models;

namespace ThiagoStore.Controllers
{
    [Authorize]
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        // GET: api/Orders - Get All orders
        [ResponseType(typeof(List<Order>))]
        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        [Route("orders")]
        public List<Order> GetOrders()
        {
            return db.Orders.Include(order => order.OrderItems).ToList();
        }



        // GET: api/Orders/5
        [ResponseType(typeof(Order))]
        [Authorize]
        public IHttpActionResult GetOrder(int id)
        {
            Order order = db.Orders.Find(id);

            // Order not found
            if (order == null)
            {
                return NotFound();
            }

            // Check user email or admin
            if (User.Identity.Name != order.userEmail || !User.IsInRole("ADMIN")) {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // Order ok
            return Ok(order);
        }

        // PUT: api/Orders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOrder(int id, Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != order.Id)
            {
                return BadRequest();
            }

            db.Entry(order).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Orders
        [ResponseType(typeof(Order))]
        [Authorize]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // infos iniciais do pedido
            order.statusPedido = "novo";
            order.pesoTotal = 0;
            order.precoFrete = 0;
            order.precoTotal = 0;
            order.dataPedido = DateTime.Today.ToString(); // data atual, criação do pedido

            // adiciona o novo pedido no banco
            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);

            if (order == null)
            {
                return NotFound();
            }

            // Check user email or admin
            if (User.Identity.Name != order.userEmail || !User.IsInRole("ADMIN"))
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            db.Orders.Remove(order);
            db.SaveChanges();

            return Ok(order);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }

        // *** Novos Métodos ***

        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("frete")]
        public IHttpActionResult CalculaFrete(string cepDestino, decimal peso, decimal comprimento,
                                decimal altura, decimal largura, decimal diametro, decimal valor,
                                Order order)
        {
            string frete;

            CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();

            // cep origem - Santa Rita 37540000
            cResultado resultado = correios.CalcPrecoPrazo("", "", "40010", "37540000", cepDestino, 
                                        peso.ToString(), 1, comprimento, altura, largura, diametro,
                                        "N", valor, "S");

            if (resultado.Servicos[0].Erro.Equals("0"))
            {
                frete = "Valor do frete: " + resultado.Servicos[0].Valor + 
                        " - Prazo de entrega: " + resultado.Servicos[0].PrazoEntrega + " dia(s)";

                return Ok(frete);
            }
            else
            {
                return BadRequest("Código do erro: " + resultado.Servicos[0].Erro + "-" + 
                    resultado.Servicos[0].MsgErro);
            }
        }

        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("cep")]
        public IHttpActionResult ObtemCEP()
        {
            CRMRestClient crmClient = new CRMRestClient();

            Customer customer = crmClient.GetCustomerByEmail(User.Identity.Name);

            if (customer != null)
            {
                return Ok(customer.zip);
            }
            else
            {
                return BadRequest("Falha ao consultar o CRM");
            }
        }

        // GET: Orders by Email
        [ResponseType(typeof(List<Order>))]
        [Authorize]
        [HttpGet]
        [Route("getOrdersByEmail")]
        public IHttpActionResult GetOrdersByEmail(string email) {

            List<Order> orders = db.Orders.Where(order => order.userEmail == email).ToList();

            // Check user email or admin
            if (User.Identity.Name != email || !User.IsInRole("ADMIN"))
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            if (orders == null) {
                return NotFound();
            }

            return Ok(orders);
        }


        [Authorize]
        [HttpGet]
        [Route("getOrdersByEmail")]
        public IHttpActionResult fecharPedido() {



            return Ok();
        }




    }
}