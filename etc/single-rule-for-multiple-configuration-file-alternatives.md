


[Regex.Escape(String) Method]


```
(^deploy\.md$)|(^deploy\.vigo$)


deploy.md
deploy.vigo
deploy.mdx
deploy.vigox
qdeploy.md
qdeploy.vigo
deploy-md
deploy-vigo
```

```c#
var sb = new StringBuilder();
var first = true;

foreach (var name in new string[] { "deploy.md", "deploy.vigo" })
{
	if (first)
		first = false;
	else 
		sb.Append('|');

	sb.Append($"(^{Regex.Escape(name)}$)");
}

sb.ToString().Dump();
```

```
(^deploy\.md$)|(^deploy\.vigo$)
```










[Regex.Escape(String) Method]: https://learn.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.escape?view=net-8.0
