using Abstracciones.Interfaces.Reglas;
using Abstracciones.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Web.Pages.Vehiculos
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguracion _configuracion;
        public List<VehiculoResponse> vehiculos { get; set; }= default!;
        public IndexModel(IConfiguracion configuration)
        {
            _configuracion = configuration;
        }
        public async Task OnGet()
        {
            string endpoint = _configuracion.ObtenerMetodo("ApiEndpoints",
                "ObtenerVehiculos");
            var cliente= new HttpClient();
            var solicitud = new HttpRequestMessage(HttpMethod.Get, endpoint);

            var respuesta = await cliente.SendAsync(solicitud);
            respuesta.EnsureSuccessStatusCode();
            var resultado = await respuesta.Content.ReadAsStringAsync();
            var opciones = new JsonSerializerOptions
            { PropertyNameCaseInsensitive = true};
            vehiculos= JsonSerializer.Deserialize<List<VehiculoResponse>>
                (resultado, opciones);
        }
    }
}
