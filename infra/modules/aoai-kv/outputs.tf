output "resource_group_name" { value = azurerm_resource_group.rg.name }
output "key_vault_name" { value = azurerm_key_vault.kv.name }
output "openai_account_name" { value = azurerm_cognitive_account.openai.name }
output "openai_endpoint" { value = azurerm_cognitive_account.openai.endpoint }
output "openai_deployment_name" { value = azurerm_cognitive_deployment.model.name }
output "secrets" { value = { endpoint = azurerm_key_vault_secret.aoai_endpoint.name, api_key = azurerm_key_vault_secret.aoai_key.name, deployment = azurerm_key_vault_secret.aoai_deployment.name } }
