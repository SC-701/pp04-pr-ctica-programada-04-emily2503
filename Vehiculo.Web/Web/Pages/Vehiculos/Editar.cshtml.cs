using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Web.Pages.Vehiculos
{
    public class EditarModel : PageModel
    {
        private readonly IConfiguracion _configuracion;

        public EditarModel(IConfiguracion configuracion)
        {
            _configuracion = configuracion;
        }

        [BindProperty]
        public VehiculoResponse vehiculoResponse { get; set; } = new VehiculoResponse();

        [BindProperty]
        public List<SelectListItem> marcas { get; set; } = new();

        [BindProperty]
        public List<SelectListItem> modelos { get; set; } = new();

        [BindProperty]
        public Guid marcaseleccionada { get; set; }

        [BindProperty]
        public Guid modeloseleccionado { get; set; }

        public async Task<IActionResult> OnGet(Guid? id)
        {
            if (!id.HasValue || id.Value == Guid.Empty)
                return NotFound();

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerVehiculo");
            var cliente = new HttpClient();
            var solicitud = new HttpRequestMessage(HttpMethod.Get, string.Format(endpoint, id.Value));

            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            if (respuesta.StatusCode == HttpStatusCode.OK)
            {
                await ObtenerMarcas();

                var resultado = await respuesta.Content.ReadAsStringAsync();
                var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                vehiculoResponse = JsonSerializer.Deserialize<VehiculoResponse>(resultado, opciones) ?? new VehiculoResponse();

                var marcaItem = marcas.FirstOrDefault(m => m.Text == vehiculoResponse.Marca);
                if (marcaItem != null && Guid.TryParse(marcaItem.Value, out Guid idMarca))
                {
                    marcaseleccionada = idMarca;

                    modelos = (await ObtenerModelos(marcaseleccionada)).Select(m =>
                        new SelectListItem
                        {
                            Value = m.Id.ToString(),
                            Text = m.Nombre,
                            Selected = m.Nombre == vehiculoResponse.Modelo
                        }
                    ).ToList();

                    var modeloItem = modelos.FirstOrDefault(m => m.Text == vehiculoResponse.Modelo);
                    if (modeloItem != null && Guid.TryParse(modeloItem.Value, out Guid idModelo))
                        modeloseleccionado = idModelo;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(Guid? id)
        {
            if (!id.HasValue || id.Value == Guid.Empty)
                return NotFound();

            var request = new VehiculoRequest
            {
                Placa = vehiculoResponse.Placa,
                Precio = vehiculoResponse.Precio,
                Anio = vehiculoResponse.Anio,
                Color = vehiculoResponse.Color,
                CorreoPropietario = vehiculoResponse.CorreoPropietario,
                TelefonoPropietario = vehiculoResponse.TelefonoPropietario,
                IdModelo = modeloseleccionado
            };

            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "EditarVehiculo");
            var cliente = new HttpClient();

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var solicitud = new HttpRequestMessage(HttpMethod.Put, string.Format(endpoint, id.Value))
            {
                Content = content
            };

            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            return RedirectToPage("./Index");
        }

        private async Task ObtenerMarcas()
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerMarcas");
            var cliente = new HttpClient();
            var solicitud = new HttpRequestMessage(HttpMethod.Get, endpoint);

            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            var resultado = await respuesta.Content.ReadAsStringAsync();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var resultadoDeserializado = JsonSerializer.Deserialize<List<Marca>>(resultado, opciones) ?? new();

            marcas = resultadoDeserializado
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Nombre
                })
                .ToList();
        }

        private async Task<List<Modelo>> ObtenerModelos(Guid marcaID)
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndPoints", "ObtenerModelos");
            var cliente = new HttpClient();

            var solicitud = new HttpRequestMessage(HttpMethod.Get, string.Format(endpoint, marcaID));

            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();

            var resultado = await respuesta.Content.ReadAsStringAsync();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return JsonSerializer.Deserialize<List<Modelo>>(resultado, opciones) ?? new();
        }

        public async Task<JsonResult> OnGetObtenerModelos(Guid marcaID)
        {
            var modelos = await ObtenerModelos(marcaID);
            return new JsonResult(modelos);
        }
    }
}
