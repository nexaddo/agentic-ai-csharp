terraform {
  required_providers {
    azurerm = { source = "hashicorp/azurerm", version = "~> 3.115" }
  }
}
provider "azurerm" { features {} }

module "aoai_kv" {
  source = "../../modules/aoai-kv"
  tenant_id  = var.tenant_id
  name_prefix = var.name_prefix
  location    = var.location
  tags        = var.tags
  openai_sku  = var.openai_sku
  model_deployment_name = var.model_deployment_name
  model_name = var.model_name
  model_version = var.model_version
  model_capacity = var.model_capacity
  secret_names = var.secret_names
}

output "prod_outputs" {
  value = {
    key_vault_name  = module.aoai_kv.key_vault_name
    openai_endpoint = module.aoai_kv.openai_endpoint
    secrets         = module.aoai_kv.secrets
  }
}
