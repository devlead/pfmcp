#!/usr/bin/env node
/**
 * Dev/CI oracle: run official pagefind.js against a bundle and write goldens.
 * Not used by pfmcp at runtime. Requires Node + a Pagefind-built index (wasm.*.pagefind).
 *
 * Usage:
 *   node generate.mjs --entry https://www.devlead.se/pagefind/pagefind-entry.json --out live
 *   node generate.mjs --dir ../pagefind --out fixture
 */
import { mkdir, writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";
import { createRequire } from "node:module";

const queries = ["cake", "cakes", "rebasing", "Cake.", "cake sdk", "tool"];
const here = dirname(fileURLToPath(import.meta.url));

function arg(name, fallback) {
  const i = process.argv.indexOf(name);
  return i >= 0 ? process.argv[i + 1] : fallback;
}

const entry = arg("--entry", "https://www.devlead.se/pagefind/pagefind-entry.json");
const outName = arg("--out", "live");
const outDir = join(here, outName);

async function loadPagefindFromUrl(entryUrl) {
  const base = entryUrl.replace(/pagefind-entry\.json$/, "");
  const pagefindJs = await (await fetch(new URL("pagefind.js", base))).text();
  const moduleUrl = `data:text/javascript,${encodeURIComponent(pagefindJs.replace(/import\.meta\.url/g, JSON.stringify(base + "pagefind.js")))}`;
  return import(moduleUrl);
}

async function main() {
  let pagefind;
  try {
    const require = createRequire(import.meta.url);
    const local = arg("--dir");
    if (local) {
      pagefind = await import(require.resolve(join(local, "pagefind.js")));
    } else {
      pagefind = await loadPagefindFromUrl(entry);
    }
  } catch (error) {
    console.error("Oracle skipped (pagefind.js / WASM unavailable):", error.message);
    process.exit(0);
  }

  await pagefind.options({ basePath: entry.replace(/pagefind-entry\.json$/, "") });
  await pagefind.init();

  await mkdir(outDir, { recursive: true });
  const goldens = [];
  for (const query of queries) {
    const result = await pagefind.search(query);
    const results = await Promise.all((result.results ?? []).slice(0, 12).map((r) => r.data()));
    const golden = {
      query,
      pageIds: (result.results ?? []).slice(0, 12).map((r) => r.id),
      scores: (result.results ?? []).slice(0, 12).map((r) => r.score),
      urls: results.map((r) => r.url),
    };
    goldens.push(golden);
    const file = query === "Cake." ? "cake-dot.json" : `${slug(query)}.json`;
    await writeFile(join(outDir, file), JSON.stringify(golden, null, 2) + "\n");
  }

  await writeFile(join(outDir, "queries.json"), JSON.stringify(goldens, null, 2) + "\n");
  console.log(`Wrote ${goldens.length} goldens to ${outDir}`);
}

function slug(query) {
  return query.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "") || "empty";
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
