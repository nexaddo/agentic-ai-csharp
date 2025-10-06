#!/usr/bin/env bash
set -euo pipefail
ENV_PATH="${1:-infra/environments/dev}"
TEMPLATE="${2}"
OUT="${3}"
pushd "$ENV_PATH" >/dev/null
TF_JSON=$(terraform output -json || echo '{}')
popd >/dev/null
KV_NAME=$(echo "$TF_JSON" | jq -r '.key_vault_name.value // .aoai_kv.key_vault_name // empty')
TENANT_ID=$(grep -E '^tenant_id' -m1 "${ENV_PATH}/terraform.tfvars" 2>/dev/null | sed -E 's/.*=\s*"(.*)".*//' || true)
SECRET_NAME_ENDPOINT=$(echo "$TF_JSON" | jq -r '.secrets.value.endpoint // .aoai_kv.secrets.value.endpoint // "AOAI-ENDPOINT"')
SECRET_NAME_APIKEY=$(echo "$TF_JSON" | jq -r '.secrets.value.api_key // .aoai_kv.secrets.value.api_key // "AOAI-API-KEY"')
SECRET_NAME_DEPLOYMENT=$(echo "$TF_JSON" | jq -r '.secrets.value.deployment // .aoai_kv.secrets.value.deployment // "AOAI-DEPLOYMENT"')
export KV_NAME TENANT_ID SECRET_NAME_ENDPOINT SECRET_NAME_APIKEY SECRET_NAME_DEPLOYMENT
envsubst < "${TEMPLATE}" > "${OUT}"
echo "Rendered ${OUT}"
