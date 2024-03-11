using Microsoft.AspNetCore.Mvc;
using MvcCorePaginacionRegistros.Models;
using MvcCorePaginacionRegistros.Repositories;

namespace MvcCorePaginacionRegistros.Controllers
{
    public class EmpleadosController : Controller
    {
        private List<Departamento> departamentos;
        private RepositoryHospital repo;

        public EmpleadosController(RepositoryHospital repo)
        {
            this.repo = repo;
        }

        public IActionResult Index()
        {
            return View(departamentos);
        }

        public async Task<IActionResult> Details(int idDepartamento)
        {
            List<Empleado> empleados = await this.repo.GetEmpleadosDepartamentoAsync(idDepartamento);
            return View(empleados);
        }
    }
}
