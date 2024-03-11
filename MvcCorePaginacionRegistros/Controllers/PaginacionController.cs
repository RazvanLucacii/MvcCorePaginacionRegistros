using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MvcCorePaginacionRegistros.Models;
using MvcCorePaginacionRegistros.Repositories;

#region VISTAS Y PROCEDIMIENTOS

//create view V_DEPARTAMENTOS_INDIVIDUAL
//as
//	select CAST(
//	ROW_NUMBER() over (order BY DEPT_NO) as int) as POSICION,
//    isnull(DEPT_NO, 0) as DEPT_NO, DNOMBRE, LOC from DEPT
//go

//create procedure SP_GRUPO_DEPARTAMENTOS
//(@posicion int)
//as
//	select DEPT_NO, DNOMBRE, LOC
//	from V_DEPARTAMENTOS_INDIVIDUAL
//	where POSICION >= @posicion and POSICION < (@posicion + 2)
//go

//create view V_GRUPO_EMPLEADOS
//as
//	select CAST(
//	ROW_NUMBER() over (order by EMP_NO) as int) as POSICION,
//    isnull(EMP_NO, 0) as EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO from EMP
//go

//create procedure SP_GRUPO_EMPLEADOS
//(@posicion int)
//as
//	select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO
//	from V_GRUPO_EMPLEADOS
//	where POSICION >= @posicion and POSICION < (@posicion + 3)
//go

#endregion

namespace MvcCorePaginacionRegistros.Controllers
{
    public class PaginacionController : Controller
    {
        private RepositoryHospital repo;

        public PaginacionController(RepositoryHospital repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> PaginarRegistroVistaDepartamento(int? posicion)
        {
            if(posicion == null)
            {
                //ponemos la posicion en el primer registro
                posicion = 1;
            }
            int numeroRegistros = await this.repo.GetNumeroRegistrosVistaDepartamentos();

            int siguiente = posicion.Value + 1;
            if(siguiente > numeroRegistros)
            {
                siguiente = numeroRegistros;
            }

            int anterior = posicion.Value - 1;
            if (anterior < 1)
            {
                anterior = 1;
            }

            VistaDepartamento vista = await this.repo.GetVistaDepartamentoAsync(posicion.Value);
            ViewData["ULTIMO"] = numeroRegistros;
            ViewData["SIGUIENTE"] = siguiente;
            ViewData["ANTERIOR"] = anterior;
            return View(vista);
        }

        public async Task<IActionResult>
            PaginarGrupoVistaDepartamento(int? posicion)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            int numeroRegistros = await
                this.repo.GetNumeroRegistrosVistaDepartamentos();
            ViewData["REGISTROS"] = numeroRegistros;
            List<VistaDepartamento> departamentos =
                await this.repo.GetGrupoVistaDepartamentoAsync(posicion.Value);
            return View(departamentos);
        }

        public async Task<IActionResult>
            PaginarGrupoDepartamentos(int? posicion)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            int numeroRegistros = await
                this.repo.GetNumeroRegistrosVistaDepartamentos();
            ViewData["REGISTROS"] = numeroRegistros;
            List<Departamento> departamentos = await
                this.repo.GetGrupoDepartamentosAsync(posicion.Value);
            return View(departamentos);
        }

        public async Task<IActionResult> PaginarGrupoEmpleados(int? posicion)
        {
            if (posicion == null)
            {
                posicion = 1;
            }
            int numeroRegistros = await
                this.repo.GetNumeroRegistrosVistaEmpleados();
            ViewData["REGISTROS"] = numeroRegistros;
            List<Empleado> empleados = await
                this.repo.GetGrupoEmpleadosAsync(posicion.Value);
            return View(empleados);
        }
    }
}
