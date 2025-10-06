# ----------------------------------------------------------
# Config
# ----------------------------------------------------------
IMG       ?= localhost/ai-support-agent:local
NAME      ?= ai-support-agent
ENV_FILE  ?= podman.env
DATA_DIR  ?= /var/lib/ai-support-agent/data
PORT      ?= 5000

KUSTOMIZE ?= kustomize
K8S_OVERLAYS_DIR ?= deploy/overlays
K8S_STAGING_NS   ?= ai-support-staging
K8S_PROD_NS      ?= ai-support-prod

K8S_SECRET_STORE_DIR ?= deploy/secrets-store

SPC_STAGING_TMPL ?= deploy/overlays/staging/secretproviderclass-azure-kv.tmpl.yaml
SPC_STAGING_OUT  ?= deploy/overlays/staging/secretproviderclass-azure-kv.yaml
SPC_PROD_TMPL    ?= deploy/overlays/production/secretproviderclass-azure-kv.tmpl.yaml
SPC_PROD_OUT     ?= deploy/overlays/production/secretproviderclass-azure-kv.yaml

# ----------------------------------------------------------
# Default target
# ----------------------------------------------------------
.DEFAULT_GOAL := help

.PHONY: help
help:
	@echo "Podman targets:"
	@echo "  make podman-build       - build local image"
	@echo "  make podman-run         - build and run single container"
	@echo "  make podman-up          - create Pod via play kube"
	@echo "  make podman-down        - stop & remove pod"
	@echo "  make podman-logs        - tail container logs"
	@echo "  make podman-status      - show container status"
	@echo "  make podman-shell       - shell into container"
	@echo "  make podman-systemd     - generate & enable systemd service"
	@echo
	@echo "Kubernetes targets:"
	@echo "  make k8s-up-staging     - apply staging overlay"
	@echo "  make k8s-down-staging   - delete staging overlay"
	@echo "  make k8s-up-prod        - apply production overlay"
	@echo "  make k8s-down-prod      - delete production overlay"
	@echo "  make k8s-status         - list pods in staging + prod namespaces"
	@echo
	@echo "Local CI:"
	@echo "  make ci                 - restore, build, test solution"
	@echo "  make ci-clean           - clean build/test artifacts"
	@echo "  make ci-results         - list test results"
	@echo
	@echo "Secrets Store (CSI Driver + Azure provider):"
	@echo "  make secrets-store-up   - install driver/provider via kustomize"
	@echo "  make secrets-store-down - remove driver/provider"
	@echo "  make secrets-store-status - show driver/provider pods"
	@echo
	@echo "SecretProviderClass (SPC) from Terraform outputs:"
	@echo "  make spc-gen-staging    - render SPC yaml for staging"
	@echo "  make spc-gen-prod       - render SPC yaml for prod"
	@echo "  make spc-apply-staging  - render & apply SPC in staging"
	@echo "  make spc-apply-prod     - render & apply SPC in prod"

# ----------------------------------------------------------
# Podman
# ----------------------------------------------------------
podman-build:
	@echo "⏳ Building image $(IMG)..."
	podman build -t $(IMG) -f Containerfile .

podman-run: podman-build
	@echo "🧹 Removing old container..."
	-podman rm -f $(NAME)
	@echo "🚀 Running $(NAME) on port $(PORT)..."
	sudo mkdir -p $(DATA_DIR) && sudo chmod -R 777 $(DATA_DIR)
	podman run -d --name $(NAME) \
	  --env-file $(ENV_FILE) \
	  -p $(PORT):5000 \
	  -v $(DATA_DIR):/data:Z \
	  $(IMG)

podman-up: podman-build
	@echo "🚀 Creating pod from kube manifest..."
	sudo mkdir -p $(DATA_DIR) && sudo chmod -R 777 $(DATA_DIR)
	podman play kube podman/kube-pod.yaml --env-file $(ENV_FILE)

podman-down:
	@echo "🛑 Stopping/removing pod $(NAME)..."
	-podman pod stop $(NAME)
	-podman pod rm $(NAME)
	@echo "ℹ️ Data remains in $(DATA_DIR)"

podman-logs:
	@podman logs -f $(NAME)

podman-status:
	@podman ps -a --filter "name=$(NAME)" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

podman-shell:
	@podman exec -it $(NAME) /bin/bash || podman exec -it $(NAME) /bin/sh

podman-systemd:
	./scripts/podman-gensystemd.sh

# ----------------------------------------------------------
# Kubernetes
# ----------------------------------------------------------
k8s-up-staging:
	@echo "🚀 Deploying staging overlay..."
	kubectl apply -k $(K8S_OVERLAYS_DIR)/staging

k8s-down-staging:
	@echo "🛑 Removing staging overlay..."
	kubectl delete -k $(K8S_OVERLAYS_DIR)/staging

k8s-up-prod:
	@echo "🚀 Deploying production overlay..."
	kubectl apply -k $(K8S_OVERLAYS_DIR)/production

k8s-down-prod:
	@echo "🛑 Removing production overlay..."
	kubectl delete -k $(K8S_OVERLAYS_DIR)/production

k8s-status:
	@echo "📊 Pods in staging namespace ($(K8S_STAGING_NS)):"
	-kubectl get pods -n $(K8S_STAGING_NS)
	@echo
	@echo "📊 Pods in production namespace ($(K8S_PROD_NS)):"
	-kubectl get pods -n $(K8S_PROD_NS)

# ----------------------------------------------------------
# Local CI shortcuts
# ----------------------------------------------------------
ci:
	@echo "🔁 Local CI: restore, build, test"
	dotnet restore SupportAgent.sln
	dotnet build --no-restore -c Release SupportAgent.sln
	dotnet test  --no-build   -c Release --logger trx --results-directory TestResults

ci-clean:
	@echo "🧹 Cleaning build/test artifacts"
	-rm -rf TestResults
	-dotnet clean SupportAgent.sln

ci-results:
	@echo "📦 Listing test results"
	@ls -lah TestResults || true

# ----------------------------------------------------------
# Secrets Store (CSI Driver + Azure Provider)
# ----------------------------------------------------------
secrets-store-up:
	@echo "🚀 Installing Secrets Store CSI Driver + Azure provider via kustomize…"
	kubectl apply -k $(K8S_SECRET_STORE_DIR)

secrets-store-down:
	@echo "🛑 Removing Secrets Store CSI Driver + Azure provider…"
	kubectl delete -k $(K8S_SECRET_STORE_DIR)

secrets-store-status:
	@echo "📊 Driver:"
	-kubectl -n kube-system get pods -l app=secrets-store-csi-driver
	@echo "📊 Azure provider:"
	-kubectl -n kube-system get pods -l app=csi-secrets-store-provider-azure

# ----------------------------------------------------------
# SecretProviderClass generation (Terraform → SPC)
# ----------------------------------------------------------
spc-gen-staging:
	@scripts/gen-spc.sh infra/environments/staging $(SPC_STAGING_TMPL) $(SPC_STAGING_OUT)

spc-gen-prod:
	@scripts/gen-spc.sh infra/environments/prod $(SPC_PROD_TMPL) $(SPC_PROD_OUT)

spc-apply-staging: spc-gen-staging
	kubectl apply -f $(SPC_STAGING_OUT)

spc-apply-prod: spc-gen-prod
	kubectl apply -f $(SPC_PROD_OUT)
