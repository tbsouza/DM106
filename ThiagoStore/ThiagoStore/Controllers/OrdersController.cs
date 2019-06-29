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
            order.precoFrete = "0";
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
        public IHttpActionResult GetFrete(string cepDestino, decimal peso, decimal comprimento,
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

                // atribui o valor e prazo do frete a order
                order.precoFrete = resultado.Servicos[0].Valor;
                order.dataEntrega = order.dataPedido + resultado.Servicos[0].PrazoEntrega;

                frete = "Valor do frete: " + resultado.Servicos[0].Valor + 
                        " - Prazo de entrega: " + resultado.Servicos[0].PrazoEntrega + " dia(s)";

                return Ok(frete);
            }
            
            return BadRequest("Código do erro: " + resultado.Servicos[0].Erro + "-" + 
                    resultado.Servicos[0].MsgErro);
        }

        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("cep")]
        public IHttpActionResult ObtemCEP()
        {
            CRMRestClient crmClient = new CRMRestClient();

            Customer customer = crmClient.GetCustomerByEmail(User.Identity.Name);

            if (customer == null) {
                return BadRequest("Falha ao consultar o CRM");
            }

            return Ok(customer.zip);
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


        [HttpGet]
        [Authorize]
        [Route("getOrdersByEmail")]
        public IHttpActionResult fecharPedido(int orderId) {

            Order order = db.Orders.Find(orderId);

            // Order not found
            if (order == null)
            {
                return NotFound();
            }

            // Check user email or admin
            if (User.Identity.Name != order.userEmail || !User.IsInRole("ADMIN"))
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // Se o frete não foi calcuado, pedido não pode ser fechado
            if (!order.precoFrete.Equals("0") ){
                return Content(HttpStatusCode.BadRequest, "Frete ainda não foi calculado para a order.");
            }

            // Atualiza status para fechado
            order.statusPedido = "fechado";

            return Content(HttpStatusCode.OK, "Pedido fechado com sucesso.");
        }

        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("calculaFrete")]
        [Authorize]
        public IHttpActionResult calculaFrete(int pedidoId) {

            // Pega a order a partir do ID
            Order order = db.Orders.Find(pedidoId);

            // Check for user authorization
            if (User.Identity.Name != order.userEmail || !User.IsInRole("ADMIN"))
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // Pega o cep do user no CRM
            var cep = ObtemCEP().ToString();
            if (cep == null) {
                return Content(HttpStatusCode.NotFound, "Impossível acessar serviço de CRM.");
            }

            // verifica se o pedido possuir order items
            if (order.OrderItems.Count == 0) {
                return Content(HttpStatusCode.BadRequest, "Order não possui nenhum order item.");
            }

            // itera pelos order items para gerar as dimensoes
            decimal pesoTotal = 0;
            decimal alturaMax = 0;
            decimal larguraMax = 0;
            decimal compTotal = 0;
            decimal precoTotal = 0;
            decimal diametroMax = 0;

            foreach (var item in order.OrderItems) {

                // Pega o produto do item
                Product produto = item.Product;

                if (produto.altura > alturaMax) {
                    alturaMax = produto.altura;
                }

                if (produto.largura > larguraMax ) {
                    larguraMax = produto.largura;
                }

                compTotal += produto.comprimento;
                precoTotal += produto.preco;
                pesoTotal += produto.peso;
                diametroMax += produto.diametro;
            }

            // Chama servico dos correios
            return GetFrete(cep, pesoTotal, compTotal, alturaMax, larguraMax, diametroMax, precoTotal, order);
        
        }

    }
}