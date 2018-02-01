Deprecated; New home : UtiLib.Net.Discovery ( https://github.com/J-kit/UtiLib)

# JScan
Allows you to scan your whole network for specific ports in a matter of seconds

```csharp
using JScan.Net.Data;
using JScan.Net.Scan;
```


#Fast Portscanning
The library also supports scanning of the whole current lan ip address range

##Scanning the whole subnet synchronously
```csharp
	ScanSettings ScS = new ScanSettings(
		   scanPorts: new UInt16[] { 3000 },
		   ipmode: IPScanMode.AllSubnet,
		   mode: ScanMode.Synchronous,
		   pingtimeout: (int)TimeSpan.FromSeconds(2).TotalMilliseconds
   );

	ScanWrapper sw = new ScanWrapper(ScS);
	sw.ExecuteScan();
	Console.WriteLine("Scan finished syncronously");
	var res = sw.Results;

	foreach (var cIP in res.Keys)
	{
		var cres = res[cIP];
		Console.WriteLine("{0}:", cIP);
		foreach (var port in cres.Keys)
		{
			Console.WriteLine("\t{0}:{1}", port, cres[port].ToString());
		}
	}
	Console.ReadLine();
```
##Scanning the whole subnet asynchronously
```csharp
	ScanSettings ScS = new ScanSettings(
		 scanPorts: new UInt16[] { 3000 },
		 ipmode: IPScanMode.AllSubnet,
		 mode: ScanMode.AsyncProgressive,
		 pingtimeout: (int)TimeSpan.FromSeconds(2).TotalMilliseconds
   );

	ScS.progressiveAsyncScanStatusChangedCallback = new Action<TCPScan>((TCPScan ar) =>
	{
		Console.WriteLine("{0}:{1} {2}", ar.Host, ar.Port, ar.TcpState);
	});
	ScS.completeAsyncScanFinishedCallback = new Action<Dictionary<IPAddress, Dictionary<int, TCPortState>>>(ar =>
	{
		Console.WriteLine("Finished Scan asyncronously");
	});
	ScanWrapper sw = new ScanWrapper(ScS);
	sw.ExecuteScan();
	Console.ReadLine();
```

##Scan Settings
Scan all subnets the machine is in
```csharp
ScanSettings ScS = new ScanSettings(
		   scanPorts: new UInt16[] { 3000 }, 							//Ports to scan
		   ipmode: IPScanMode.AllSubnet, 								//Scan all ips in the subnet the machine is in, doesn't need any additional data
		   mode: ScanMode.Synchronous,  								//Don't run async, wait for result, the results are avaiable afterwards in the apropriate property
		   pingtimeout: (int)TimeSpan.FromSeconds(2).TotalMilliseconds 	//Set the maximal ping timeout, 2 seconds are recommended
);
```
Scan just a specific netmask
```csharp
ScanSettings ScS = new ScanSettings(
	   scanPorts: new UInt16[] { 80 },
	   ipmode: IPScanMode.Subnet, 																		//Scan just specific subnets, MaskData is required!
	   mode: ScanMode.AsyncProgressive, 																//Report status on statuschange and call completition action (needs to be set)
	   pingtimeout: (int)TimeSpan.FromSeconds(2).TotalMilliseconds,
	   StorageData: (IScanStorage)new List<AddressByteCollection>(new[] {
		   new AddressByteCollection(IPAddress.Parse("10.0.0.1"), IPAddress.Parse("255.255.255.0"))		//Required: a ip in the network and the mask
	   })
);
```

Scan specific list of ip's
```csharp
	ScanSettings ScS = new ScanSettings(
		   scanPorts: new UInt16[] { 80 },
		   ipmode: IPScanMode.List,					//Scan just specific list of ips (ScanStorageListData required!!)
		   mode: ScanMode.Synchronous,
		   pingtimeout: (int)TimeSpan.FromSeconds(2).TotalMilliseconds,
		   StorageData: (IScanStorage)new ScanStorageListData(new[] { //needed if ipmode is IPScanMode.List
			   IPAddress.Parse("10.0.0.1"),
			   IPAddress.Parse("10.0.0.2"),
		   })
	);
```
Scan ip ranges
```csharp
//Not implemented yet!
```
