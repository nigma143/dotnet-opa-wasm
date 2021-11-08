﻿using Opa.Wasm;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

EvaluateHelloWorld();
EvaluateRbac();
ReadFromBundle();

Console.Read();

// https://play.openpolicyagent.org/ "Role-based" example stripped down to minimum
static void EvaluateRbac()
{
	using var opaRuntime = new OpaRuntime();
	using var module = opaRuntime.Load("rbac.wasm");

	// Now you can create as many instances of OpaPolicy on top of this runtime & loaded module as you want
	using var opaPolicy = new OpaPolicy(opaRuntime, module);

	opaPolicy.SetData(@"{""user_roles"": { ""alice"": [""admin""],""bob"": [""employee"",""billing""],""eve"": [""customer""]}}");

	string input = @"{ ""user"": ""alice"", ""action"": ""read"", ""object"": ""id123"", ""type"": ""dog"" }";
	string output = opaPolicy.Evaluate(input);

	Console.WriteLine($"RBAC output: {output}");
}

static void EvaluateHelloWorld()
{
	// "One-off" evaluations can be done without explicitly creating & keeping around of an OpaRuntime
	using var opaPolicy = new OpaPolicy("example.wasm");

	opaPolicy.SetData(@"{""world"": ""world""}");

	string input = @"{""message"": ""world""}";
	string output = opaPolicy.Evaluate(input);

	Console.WriteLine($"Hello world output: {output}");
}

static void ReadFromBundle()
{
	using var inStream = File.OpenRead("bundle-example.tar.gz"); // by default would be bundle.tar.gz
	using var gzipStream = new GZipInputStream(inStream);
	using var tarStream = new TarInputStream(gzipStream, null);

	TarEntry current = null;
	MemoryStream ms = null;
	while (null != (current = tarStream.GetNextEntry()))
	{
		if ("/policy.wasm" == current.Name)
		{
			ms = new MemoryStream();
			tarStream.CopyEntryContents(ms);
			break;
		}
	}

	tarStream.Close();
	gzipStream.Close();
	inStream.Close();

	if (null != ms)
	{
		ms.Position = 0;
		var bytes = ms.ToArray();
		int length = bytes.Length; // 116020
	}
}
