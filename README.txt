The Customer FX SalesProcessAdapter is an assembly that will handle all aspects of 
the creation of SalesLogix sales processes.

To use the SalesProcessAdapter, you must add the FX.SalesProcess.SalesProcessAdapter.dll
assembly to your project as well as Sublogix.dll


using FX.SalesProcess;
//...


// Create the adapter, passing in either a
// connection string or a Sublogix repository

var procAdapter = new SalesProcessAdapter(ConnectionString);

// Call CreateProcess passing in an opportunityid and the pluginid
// of the sales process plugin record
procAdapter.CreateProcess(oppId, pluginId);


For issues or questions see Ryan Farley

