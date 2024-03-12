using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MvcCorePaginacionRegistros.Data;
using MvcCorePaginacionRegistros.Models;
using System.Data;
using System.Linq;

#region

//create procedure SP_REGISTRO_EMPLEADO_DEPARTAMENTO
//(@posicion int, @departamento int
//, @registros int out)
//as
//select @registros = count(EMP_NO) from EMP
//where DEPT_NO=@departamento
//select EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO from 
//    (select cast(
//    ROW_NUMBER() OVER (ORDER BY APELLIDO) as int) AS POSICION
//    , EMP_NO, APELLIDO, OFICIO, SALARIO, DEPT_NO
//    from EMP
//    where DEPT_NO=@departamento) as QUERY
//    where QUERY.POSICION = @posicion
//go

#endregion

namespace MvcCorePaginacionRegistros.Repositories
{
    public class RepositoryHospital
    {
        private HospitalContext context;

        public RepositoryHospital(HospitalContext context)
        {
            this.context = context;
        }

        public async Task<List<Departamento>> GetDepartamentosAsync()
        {
            return await this.context.Departamentos.ToListAsync();
        }

        public async Task<Departamento> FindDepartamentoAsync(int idDepartamento)
        {
            return await this.context.Departamentos
                .FirstOrDefaultAsync(x => x.IdDepartamento == idDepartamento);
        }

        public async Task<ModelEmpleadoPaginacion>
            GetEmpleadoDepartamentoAsync
            (int posicion, int iddepartamento)
        {
            string sql = "SP_REGISTRO_EMPLEADO_DEPARTAMENTO @posicion, @departamento, "
                + " @registros out";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);
            SqlParameter pamDepartamento =
                new SqlParameter("@departamento", iddepartamento);
            SqlParameter pamRegistros = new SqlParameter("@registros", -1);
            pamRegistros.Direction = ParameterDirection.Output;
            var consulta =
                this.context.Empleados.FromSqlRaw
                (sql, pamPosicion, pamDepartamento, pamRegistros);
            //PRIMERO DEBEMOS EJECUTAR LA CONSULTA PARA PODER RECUPERAR 
            //LOS PARAMETROS DE SALIDA
            var datos = await consulta.ToListAsync();
            Empleado empleado = datos.FirstOrDefault();
            int registros = (int)pamRegistros.Value;
            return new ModelEmpleadoPaginacion
            {
                Registros = registros,
                Empleado = empleado
            };
        }

        public async Task<List<Empleado>> GetEmpleadosDepartamentoAsync(int idDepartamento)
        {
            var empleados = this.context.Empleados.Where(x => x.IdDepartamento == idDepartamento);
            if(empleados.Count() == 0)
            {
                return null;
            }
            else
            {
                return await empleados.ToListAsync();
            }
        }

        public async Task<List<VistaDepartamento>> GetGrupoVistaDepartamentoAsync(int posicion)
        {
            var consulta = from datos in this.context.viewDept
                           where datos.Posicion >= posicion
                           && datos.Posicion < (posicion + 2)
                           select datos;
            return await consulta.ToListAsync();
        }

        public async Task<int> GetNumeroRegistrosVistaDepartamentosAsync()
        {
            return await this.context.Departamentos.CountAsync();
        }

        public async Task<int> GetNumeroEmpleadosAsync()
        {
            return await this.context.Empleados.CountAsync();
        }

        public async Task<VistaDepartamento> GetVistaDepartamentoAsync(int posicion)
        {
            VistaDepartamento? vista = await this.context.viewDept.Where(z => z.Posicion == posicion).FirstOrDefaultAsync();
            return vista;
        }

        public async Task<int> GetNumeroEmpleadosOficioAsync(string oficio)
        {
            return await this.context.Empleados
                .Where(z => z.Oficio == oficio).CountAsync();
        }

        public async Task<List<Empleado>> GetGrupoEmpleadosOficioAsync
            (int posicion, string oficio)
        {
            string sql = "SP_GRUPO_EMPLEADOS_OFICIOS @posicion, @oficio";
            SqlParameter pamPosicion =
                new SqlParameter("@posicion", posicion);
            SqlParameter pamOficio =
                new SqlParameter("@oficio", oficio);
            var consulta = this.context.Empleados.FromSqlRaw
                (sql, pamPosicion, pamOficio);
            return await consulta.ToListAsync();
        }

        //el controler nos va a dar una posicion y un oficio
        //debemos devolver los empleados y el numero de registros
        public async Task<ModelPaginacionEmpleados> GetGrupoEmpleadosOficioOutAsync(int posicion, string oficio)
        {
            string sql = "SP_GRUPO_EMPLEADOS_OFICIO_OUT @posicion, @oficio, "
                + " @registros out";
            SqlParameter pamPosicion = new SqlParameter("@posicion", posicion);
            SqlParameter pamOficio = new SqlParameter("@oficio", oficio);
            SqlParameter pamRegistros = new SqlParameter("@registros", -1);
            pamRegistros.Direction = ParameterDirection.Output;
            var consulta =
                this.context.Empleados.FromSqlRaw
                (sql, pamPosicion, pamOficio, pamRegistros);
            //PRIMERO DEBEMOS EJECUTAR LA CONSULTA PARA PODER RECUPERAR 
            //LOS PARAMETROS DE SALIDA
            List<Empleado> empleados = await consulta.ToListAsync();
            int registros = (int)pamRegistros.Value;
            return new ModelPaginacionEmpleados
            {
                NumeroRegistros = registros,
                Empleados = empleados
            };
        }

        public async Task<List<Departamento>>
            GetGrupoDepartamentosAsync(int posicion)
        {
            string sql = "SP_GRUPO_DEPARTAMENTOS @posicion";
            SqlParameter pamPosicion =
                new SqlParameter("posicion", posicion);
            var consulta =
                this.context.Departamentos.FromSqlRaw(sql, pamPosicion);
            return await consulta.ToListAsync();
        }

        public async Task<List<Empleado>>
            GetGrupoEmpleadosAsync(int posicion)
        {
            string sql = "SP_GRUPO_EMPLEADOS @posicion";
            SqlParameter pamPosicion =
                new SqlParameter("posicion", posicion);
            var consulta =
                this.context.Empleados.FromSqlRaw(sql, pamPosicion);
            return await consulta.ToListAsync();
        }
    }
}
