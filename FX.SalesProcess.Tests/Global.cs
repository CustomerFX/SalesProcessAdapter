using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FX.SalesProcess.Tests
{
	public class Global
	{
		// Tests expected to be performed with SalesLogix EVAL database on localhost
		// Mocks will come later :-)

		public static string ConnectionString = "Provider=SLXOLEDB.1;Data Source=localhost;Persist Security Info=True;User ID=Admin;Initial Catalog=SALESLOGIX_EVAL;Extended Properties=\"PORT=1706;LOG=ON;CASEINSENSITIVEFIND=ON;AUTOINCBATCHSIZE=1;SVRCERT=;\"";

		public static string ProcessPluginId = "pDEMOA0000FL";
	}
}
