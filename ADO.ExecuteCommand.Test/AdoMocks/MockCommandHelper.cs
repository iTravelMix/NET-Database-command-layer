﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ADO.ExecuteCommand.Helper;
using ADO.Query.Test.AdoMocks;
using Rhino.Mocks;

namespace ADO.ExecuteCommand.Test.AdoMocks
{
    public class MockCommandHelper : CommandHelper
    {
        private IList<IDataParameter> parameters;

        public IList<IDictionary<string, object>> ReturnValues { get; set; }

        public override IDbConnection GetConnection()
        {
            var conn = MockRepository.GenerateMock<IDbConnection>();
            var dbcom = MockRepository.GenerateMock<IDbCommand>();

            dbcom.Expect(c => c.Parameters.Clear());
            dbcom.Stub(c => c.ExecuteScalar()).Return( (this.ReturnValues.Count > 0) ? this.ReturnValues[0][this.ReturnValues[0].Keys.First()] : 0);

            dbcom.Stub(c => c.ExecuteReader(CommandBehavior.CloseConnection)).Return(new MockDataReader(this.ReturnValues));
            dbcom.Stub(c => c.ExecuteReader()).Return(new MockDataReader(this.ReturnValues));

            conn.Expect(m => m.Open());
            conn.Expect(m => m.Close());
            conn.Stub(m => m.CreateCommand()).Return(dbcom);
            conn.Stub(m => m.State).Return(ConnectionState.Open);

            return conn;
        }

        protected override IDataParameter GetParameter()
        {
            var parameter = new MockParameters { Direction = ParameterDirection.Input };
            if (this.parameters == null) this.parameters = new List<IDataParameter>();

            this.parameters.Add(parameter);
            return parameter;
        }

        protected override IDbDataAdapter GetDataAdapter()
        {
            var dba = MockRepository.GenerateMock<IDbDataAdapter>();
            dba.Expect(da => da.SelectCommand);

            return dba;
        }

        protected override void DeriveParameters(IDbCommand cmd)
        {
            throw new NotImplementedException();
        }

        protected override DataTable FillTable(IDbDataAdapter da)
        {
            if (this.ReturnValues == null || !this.ReturnValues.Any()) throw new NoNullAllowedException();

            var dt = new DataTable();
            dt.Clear();

            foreach (var col in this.ReturnValues[0].Keys)
            {
                dt.Columns.Add(col);
            }

            foreach (var rows in this.ReturnValues)
            {
                var row = dt.NewRow();

                foreach (var returnValue in rows)
                {
                    row[returnValue.Key] = returnValue.Value;
                }

                dt.Rows.Add(row);
            }
            
            return dt;
        }

        public IEnumerable<IDataParameter> Parameters => this.parameters;
    }
}
