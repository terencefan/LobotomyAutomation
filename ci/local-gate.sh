#!/usr/bin/env bash
# 本地覆盖率门禁：与 CI test job 同口径——dotnet-coverage 收集 + diff-cover 对
# origin/main 差分行 ≥80%（对比 CI 的 --fail-under 80）。
# 用法：ci/local-gate.sh   （先 git fetch origin main 以供差分）
set -euo pipefail
cd "$(dirname "$0")/.."

DOTNET_COVERAGE="${DOTNET_COVERAGE:-$HOME/.dotnet/tools/dotnet-coverage}"
if [ ! -x "$DOTNET_COVERAGE" ]; then
  dotnet tool install -g dotnet-coverage
fi

DIFF_COVERAGE="${DIFF_COVERAGE:-$(command -v diff-cover || true)}"
if [ -z "$DIFF_COVERAGE" ]; then
  # 系统 python 为 PEP 668 externally-managed，走专用 venv
  DIFF_COVERAGE="$HOME/.venvs/diffcover/bin/diff-cover"
  if [ ! -x "$DIFF_COVERAGE" ]; then
    python3 -m venv "$HOME/.venvs/diffcover"
    "$HOME/.venvs/diffcover/bin/pip" install -q diff-cover
  fi
fi

(cd AutoInority.Test && "$DOTNET_COVERAGE" collect "dotnet test" -f cobertura -o ../coverage.cobertura.xml)
exec "$DIFF_COVERAGE" coverage.cobertura.xml --compare-branch origin/main --fail-under 80
