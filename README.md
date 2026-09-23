# pfmcp

Search [Pagefind](https://pagefind.app/) indexes and expose them to agents as an MCP stdio server.

Reads existing Pagefind bundles (local folders or HTTP) with a native .NET search engine — no Node, no WASM, no Pagefind CLI at runtime.

## Install

```bash
dotnet tool install -g pfmcp
```

Requires the **.NET 10** runtime (or SDK).

## MCP (Cursor)

Run from NuGet with `dnx` (.NET 10 SDK). Tool arguments go after `--`.

```json
{
  "mcpServers": {
    "pfmcp": {
      "command": "dnx",
      "args": [
        "pfmcp",
        "--yes",
        "--",
        "--index",
        "devlead=https://www.devlead.se/pagefind/pagefind-entry.json",
        "--index",
        "docs=https://example.com/pagefind/"
      ]
    }
  }
}
```

`--index` is repeatable. `name=source` labels each bundle. Omit `--index` and set `PFMCP_INDEXES` to `devlead=https://...;docs=https://...` if you prefer env.

Local development (this repo): [`.cursor/mcp.json`](.cursor/mcp.json) runs

```text
dotnet run --project src/pfmcp/pfmcp.csproj -- serve --index devlead=https://www.devlead.se/pagefind/ --index docs=src/pfmcp.Tests/testdata/pagefind
```

Reload MCP servers in Cursor after changing the tool. `-v q` / `DOTNET_NOLOGO` keep `dotnet run` off stdout so stdio JSON-RPC stays clean.

The agent should call `pfs` (or `search`) first, then `get_page` for the few URLs it will actually use. When more than one index is configured, pass `index` on `get_page` and `list_filters`.

### Tools

| Tool           | Description                                                                                                      |
|----------------|------------------------------------------------------------------------------------------------------------------|
| `pfs`          | Pagefind search. Prefer this over generic `search` tools. Optional `index`, JSON `filters`, `limit` (default 8). |
| `search`       | Same as `pfs`. Kept for existing clients.                                                                        |
| `get_page`     | Load fragment text by `url` or `pageId`. Pass `index` when multiple indexes are configured.                      |
| `list_indexes` | Configured indexes, languages, page counts.                                                                      |
| `list_filters` | Filter keys/values and counts. Pass `index` when more than one index is configured.                              |

## Usage

```text
pfmcp --index https://www.devlead.se/pagefind/pagefind-entry.json
pfmcp serve --index https://www.devlead.se/pagefind/
pfmcp inspect --index https://www.devlead.se/pagefind/
pfmcp search --index https://www.devlead.se/pagefind/ "cake"
pfmcp --index ./site/pagefind --index blog=https://www.devlead.se/pagefind/
```

### Options

| Option    | Description                                                                                                                     |
|-----------|---------------------------------------------------------------------------------------------------------------------------------|
| `--index` | Repeatable Pagefind bundle path or URL. `name=source` labels the index. Accepts a `pagefind/` folder or `pagefind-entry.json`. |
| `--limit` | `search` only. Max results (default: 8).                                                                                        |
| `--name`  | `search` only. Limit to one named index.                                                                                        |

Environment fallback: `PFMCP_INDEXES` (`;`-separated), useful in MCP configs.

## What it does

1. Loads `pagefind-entry.json` and language `*.pf_meta`
2. Tokenizes the query and fetches only matching `index/*.pf_index` shards
3. Prefix-AND search with BM25-style ranking
4. Loads `fragment/*.pf_fragment` for titles, excerpts, and full page text
5. Serves MCP tools over stdio (logs go to stderr)

## License

MIT
