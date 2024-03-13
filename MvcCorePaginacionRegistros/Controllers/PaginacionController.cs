using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MvcCorePaginacionRegistros.Models;
using MvcCorePaginacionRegistros.Repositories;
using System.Data;

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

        public async Task<IActionResult> Departamentos()
        {
            List<Departamento> departamentos = await this.repo.GetDepartamentosAsync();
            return View(departamentos);
        }


        public async Task<IActionResult> EmpleadosDepartamento
            (int? posicion, int iddepartamento)
        {
            if (posicion == null)
            {
                //POSICION PARA EL EMPLEADO
                posicion = 1;
            }
            ModelEmpleadoPaginacion model = await
                this.repo.GetEmpleadoDepartamentoAsync
                (posicion.Value, iddepartamento);
            Departamento departamento = await this.repo.FindDepartamentoAsync(iddepartamento);
            ViewData["DEPASELECCIONADO"] = departamento;
            ViewData["DEPARTAMENTO"] = iddepartamento;            
            ViewData["POSICION"] = posicion;
            return View(model.Empleado);
        }

        public async Task<IActionResult> EmpleadoDetailsPartial(int? posicion, int iddepartamento)
        {
            if(posicion == null)
            {
                return PartialView("_EmpleadoPartial");
            }
            ModelEmpleadoPaginacion model = await this.repo.GetEmpleadoDepartamentoAsync(posicion.Value, iddepartamento);           
            ViewData["REGISTROS"] = model.Registros;
            int siguiente = posicion.Value + 1;
            //DEBEMOS COMPROBAR QUE NO PASAMOS DEL NUMERO DE REGISTROS
            if (siguiente > model.Registros)
            {
                //EFECTO OPTICO
                siguiente = model.Registros;
            }
            int anterior = posicion.Value - 1;
            if (anterior < 1)
            {
                anterior = 1;
            }
            ViewData["ULTIMO"] = model.Registros;
            ViewData["SIGUIENTE"] = siguiente;
            ViewData["ANTERIOR"] = anterior;
            ViewData["POSICION"] = posicion;
            return PartialView("_EmpleadoPartial", model.Empleado);
        }



            public async Task<IActionResult> PaginarRegistroVistaDepartamento(int? posicion)
        {
            if(posicion == null)
            {
                //ponemos la posicion en el primer registro
                posicion = 1;
            }
            int numeroRegistros = await this.repo.GetNumeroRegistrosVistaDepartamentosAsync();

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
                this.repo.GetNumeroRegistrosVistaDepartamentosAsync();
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
                this.repo.GetNumeroRegistrosVistaDepartamentosAsync();
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
                this.repo.GetNumeroEmpleadosAsync();
            ViewData["REGISTROS"] = numeroRegistros;
            List<Empleado> empleados = await
                this.repo.GetGrupoEmpleadosAsync(posicion.Value);
            return View(empleados);
        }

        public async Task<IActionResult> EmpleadosOficio(int? posicion, string oficio)
        {
            if (posicion == null)
            {
                posicion = 1;
                return View();
            }
            else
            {
                List<Empleado> empleados = await this.repo.GetGrupoEmpleadosOficioAsync(posicion.Value, oficio);
                int registros = await this.repo.GetNumeroEmpleadosOficioAsync(oficio);
                ViewData["REGISTROS"] = registros;
                ViewData["OFICIO"] = oficio;
                return View(empleados);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmpleadosOficio
            (string oficio)
        {
            //CUANDO BUSCAMOS, NORMALMENTE, EN QUE POSICION COMIENZA TODO?
            List<Empleado> empleados = await
                this.repo.GetGrupoEmpleadosOficioAsync(1, oficio);
            int registros = await this.repo.GetNumeroEmpleadosOficioAsync(oficio);
            ViewData["REGISTROS"] = registros;
            ViewData["OFICIO"] = oficio;
            return View(empleados);
        }

        public async Task<IActionResult> EmpleadosOficioOut(int? posicion, string oficio)
        {
            if (posicion == null)
            {
                posicion = 1;
                return View();
            }
            else
            {
                ModelPaginacionEmpleados model = await
                    this.repo.GetGrupoEmpleadosOficioOutAsync(posicion.Value, oficio);
                ViewData["REGISTROS"] = model.NumeroRegistros;
                ViewData["OFICIO"] = oficio;
                return View(model.Empleados);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EmpleadosOficioOut(string oficio)
        {
            ModelPaginacionEmpleados model = await
                this.repo.GetGrupoEmpleadosOficioOutAsync(1, oficio);
            ViewData["REGISTROS"] = model.NumeroRegistros;
            ViewData["OFICIO"] = oficio;
            return View(model.Empleados);
        }
    }
}
