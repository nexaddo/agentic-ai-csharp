terraform {
  required_version = ">= 1.6.0"
  required_providers {
    azurerm = { source = "hashicorp/azurerm", version = "~> 3.115" }
    random  = { source = "hashicorp/random",  version = "~> 3.6" }
  }
}
provider "azurerm" { features {} }

resource "azurerm_resource_provider_registration" "cognitiveservices" { name = "Microsoft.CognitiveServices" }

resource "random_string" "suffix" { length=5 upper=false lower=true numeric=true special=false }

resource "azurerm_resource_group" "rg" {
  name     = "${var.name_prefix}-rg-${random_string.suffix.result}"
  location = var.location
  tags     = var.tags
}

resource "azurerm_key_vault" "kv" {
  name                       = "${var.name_prefix}kv${random_string.suffix.result}"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  tenant_id                  = var.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 7
  purge_protection_enabled   = true
  enable_rbac_authorization  = true
  public_network_access_enabled = true
  tags = var.tags
}

resource "azurerm_cognitive_account" "openai" {
  name                = "${var.name_prefix}-aoai-${random_string.suffix.result}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  kind     = "OpenAI"
  sku_name = var.openai_sku
  custom_subdomain_name = "${var.name_prefix}-aoai-${random_string.suffix.result}"
  public_network_access_enabled = true
  identity { type = "SystemAssigned" }
  tags = var.tags
  depends_on = [azurerm_resource_provider_registration.cognitiveservices]
}

resource "azurerm_cognitive_deployment" "model" {
  name                 = var.model_deployment_name
  cognitive_account_id = azurerm_cognitive_account.openai.id
  model { format = "OpenAI" name = var.model_name version = var.model_version }
  scale { type = "Standard" capacity = var.model_capacity }
  tags = var.tags
}

resource "azurerm_key_vault_secret" "aoai_endpoint"   { name = var.secret_names.endpoint   value = azurerm_cognitive_account.openai.endpoint              key_vault_id = azurerm_key_vault.kv.id }
resource "azurerm_key_vault_secret" "aoai_key"        { name = var.secret_names.api_key    value = azurerm_cognitive_account.openai.primary_access_key   key_vault_id = azurerm_key_vault.kv.id }
resource "azurerm_key_vault_secret" "aoai_deployment" { name = var.secret_names.deployment value = azurerm_cognitive_deployment.model.name               key_vault_id = azurerm_key_vault.kv.id }
