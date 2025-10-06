variable "tenant_id" { type = string }
variable "name_prefix" { type = string, default = "aisupport-dev" }
variable "location" { type = string, default = "eastus" }
variable "tags" { type = map(string), default = { project="ai-support-agent", env="dev" } }
variable "openai_sku" { type = string, default = "S0" }
variable "model_deployment_name" { type = string, default = "gpt4o-mini-deploy" }
variable "model_name" { type = string, default = "gpt-4o-mini" }
variable "model_version" { type = string, default = "2024-08-06" }
variable "model_capacity" { type = number, default = 1 }
variable "secret_names" {
  type = object({ endpoint=string, api_key=string, deployment=string })
  default = { endpoint="AOAI-ENDPOINT", api_key="AOAI-API-KEY", deployment="AOAI-DEPLOYMENT" }
}
