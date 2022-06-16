Deploy custom plugin DLLs and their support files to this folder.

DO NOT deploy dependency files - like referenced assemblies, framework components, NuGet packages, etc.
(unless they are custom libraries that are not included in CodaClient).

Especially, do not deploy CodaClient.Plugin.dll here, it will break PlugIn loading.
