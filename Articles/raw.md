- add the necessary pictures. i will add later
-
# Microsoft Foundry Flex Processing: documentação oficial

Repositório analisado: [https://github.com/TallesValiatti/MsftFoundryFlexProcessing](https://github.com/TallesValiatti/MsftFoundryFlexProcessing)

O projeto demonstra o uso de Flex Processing e Standard Processing no Microsoft Foundry, utilizando .NET 10, `HttpClient`, Responses API, `service_tier`, comparação de latência, consumo de tokens, custos e fallback de Flex para Standard.

Abaixo estão as referências oficiais da Microsoft, organizadas por assunto, para serem utilizadas como fontes do artigo.

## 1. Microsoft Foundry Flex Processing

- Flex Processing Overview, Configuration and Supported Models [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing)
- Microsoft Foundry Overview [https://learn.microsoft.com/en-us/azure/foundry/what-is-foundry](https://learn.microsoft.com/en-us/azure/foundry/what-is-foundry)
- Azure OpenAI Deployment Types [https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/deployment-types](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/deployment-types)
- Azure OpenAI Model Availability [https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models)
- Azure OpenAI Quotas and Limits [https://learn.microsoft.com/en-us/azure/foundry/openai/quotas-limits](https://learn.microsoft.com/en-us/azure/foundry/openai/quotas-limits)

A documentação de Flex Processing é a principal fonte da verdade para o projeto, incluindo `service_tier: "flex"`, desconto de 50% em relação ao Standard, modelos compatíveis, ausência de SLA e tratamento de falhas.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn



## 2. Flex vs Standard vs Priority

- Flex Processing vs Standard Processing [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing)
- Priority Processing Overview and Configuration [https://learn.microsoft.com/en-us/azure/foundry/openai/concepts/priority-processing](https://learn.microsoft.com/en-us/azure/foundry/openai/concepts/priority-processing)
- Azure OpenAI Deployment Types and Processing Options [https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/deployment-types](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/deployment-types)
- Azure OpenAI Latency and Performance [https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/latency](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/latency)
- Azure OpenAI Quotas and Limits [https://learn.microsoft.com/en-us/azure/foundry/openai/quotas-limits](https://learn.microsoft.com/en-us/azure/foundry/openai/quotas-limits)

## 3. Batch Processing e Provisioned Throughput (PTU)

- Azure OpenAI Batch Processing [https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/batch](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/batch)
- Provisioned Throughput Overview [https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/provisioned-throughput](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/provisioned-throughput)
- Provisioned Throughput Quickstart [https://learn.microsoft.com/en-us/azure/foundry/openai/provisioned-quickstart](https://learn.microsoft.com/en-us/azure/foundry/openai/provisioned-quickstart)
- Provisioned Throughput Billing and Cost Management [https://learn.microsoft.com/en-us/azure/foundry/openai/concepts/provisioned-throughput-billing](https://learn.microsoft.com/en-us/azure/foundry/openai/concepts/provisioned-throughput-billing)
- Operate Provisioned Deployments in Production [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/provisioned-get-started](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/provisioned-get-started)

## 4. Responses API e Service Tier

- Azure OpenAI Responses API [https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/responses](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/responses)
- Azure OpenAI REST API Reference [https://learn.microsoft.com/en-us/azure/ai-services/openai/reference](https://learn.microsoft.com/en-us/azure/ai-services/openai/reference)
- Azure OpenAI API Versions [https://learn.microsoft.com/en-us/azure/ai-services/openai/api-version-lifecycle](https://learn.microsoft.com/en-us/azure/ai-services/openai/api-version-lifecycle)
- Azure OpenAI Chat Completions API [https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/chatgpt](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/chatgpt)
- Flex Processing with Responses API [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing)
- Override Service Tier with x-ms-service-tier Header [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing#override-the-service-tier-with-a-request-header](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing#override-the-service-tier-with-a-request-header)

## 5. Prompt Caching e consumo de tokens

- Azure OpenAI Prompt Caching [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/prompt-caching](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/prompt-caching)
- Cached Input Tokens and Cache Write Tokens [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/prompt-caching](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/prompt-caching)
- Prompt Cache Retention and Configuration [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/prompt-caching](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/prompt-caching)
- Flex Processing Token Pricing and Discounts [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing)
- Azure OpenAI REST API Usage Fields [https://learn.microsoft.com/en-us/azure/ai-services/openai/reference](https://learn.microsoft.com/en-us/azure/ai-services/openai/reference)

## 6. Custos e precificação

- Azure OpenAI Pricing [https://azure.microsoft.com/en-us/pricing/details/azure-openai/](https://azure.microsoft.com/en-us/pricing/details/azure-openai/)
- Azure Pricing Calculator [https://azure.microsoft.com/en-us/pricing/calculator/](https://azure.microsoft.com/en-us/pricing/calculator/)
- Flex Processing Pricing and Cost Monitoring [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing)
- Priority Processing Pricing and Cost Monitoring [https://learn.microsoft.com/en-us/azure/foundry/openai/concepts/priority-processing](https://learn.microsoft.com/en-us/azure/foundry/openai/concepts/priority-processing)
- Provisioned Throughput Billing [https://learn.microsoft.com/en-us/azure/foundry/openai/concepts/provisioned-throughput-billing](https://learn.microsoft.com/en-us/azure/foundry/openai/concepts/provisioned-throughput-billing)
- Microsoft Cost Management Overview [https://learn.microsoft.com/en-us/azure/cost-management-billing/costs/overview-cost-management](https://learn.microsoft.com/en-us/azure/cost-management-billing/costs/overview-cost-management)

## 7. Tratamento de erros, fallback e resiliência

- Flex Processing Error Handling (HTTP 400, 408, 429) [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing#handle-flex-errors](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing#handle-flex-errors)
- Fallback from Flex to Standard [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing#fall-back-to-standard-processing](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing#fall-back-to-standard-processing)
- Flex Processing Production Best Practices [https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing#apply-production-best-practices](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing#apply-production-best-practices)
- Azure OpenAI Quotas and Rate Limits [https://learn.microsoft.com/en-us/azure/foundry/openai/quotas-limits](https://learn.microsoft.com/en-us/azure/foundry/openai/quotas-limits)
- HttpClient Guidelines for .NET [https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)

## 8. .NET 10, HttpClient e System.Text.Json

- What's New in .NET 10 [https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10/overview)
- .NET CLI Overview [https://learn.microsoft.com/en-us/dotnet/core/tools/](https://learn.microsoft.com/en-us/dotnet/core/tools/)
- HttpClient Class [https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
- Make HTTP Requests with HttpClient [https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient)
- System.Text.Json Overview [https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview)
- Environment.GetEnvironmentVariable API [https://learn.microsoft.com/en-us/dotnet/api/system.environment.getenvironmentvariable](https://learn.microsoft.com/en-us/dotnet/api/system.environment.getenvironmentvariable)
- CancellationToken Documentation [https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtoken)

## 9. Segurança e autenticação

- Azure OpenAI Authentication [https://learn.microsoft.com/en-us/azure/ai-services/authentication](https://learn.microsoft.com/en-us/azure/ai-services/authentication)
- Microsoft Entra ID Authentication for Azure OpenAI [https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/managed-identity](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/managed-identity)
- Azure Identity .NET SDK [https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme)
- Azure RBAC Overview [https://learn.microsoft.com/en-us/azure/role-based-access-control/overview](https://learn.microsoft.com/en-us/azure/role-based-access-control/overview)
- Azure AI Services Built-in Roles [https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles/ai-machine-learning](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles/ai-machine-learning)
- Azure Key Vault Secrets Overview [https://learn.microsoft.com/en-us/azure/key-vault/secrets/about-secrets](https://learn.microsoft.com/en-us/azure/key-vault/secrets/about-secrets)

## 10. Repositório e implementação

- Microsoft Foundry Flex Processing Sample [https://github.com/TallesValiatti/MsftFoundryFlexProcessing](https://github.com/TallesValiatti/MsftFoundryFlexProcessing)
- Project README [https://github.com/TallesValiatti/MsftFoundryFlexProcessing/blob/main/README.md](https://github.com/TallesValiatti/MsftFoundryFlexProcessing/blob/main/README.md)
- .NET 10 Console Application [https://github.com/TallesValiatti/MsftFoundryFlexProcessing/tree/main/src/FlexProcessingDemo](https://github.com/TallesValiatti/MsftFoundryFlexProcessing/tree/main/src/FlexProcessingDemo)
- FoundryClient Implementation [https://github.com/TallesValiatti/MsftFoundryFlexProcessing/blob/main/src/FlexProcessingDemo/FoundryClient.cs](https://github.com/TallesValiatti/MsftFoundryFlexProcessing/blob/main/src/FlexProcessingDemo/FoundryClient.cs)
- CostCalculator Implementation [https://github.com/TallesValiatti/MsftFoundryFlexProcessing/blob/main/src/FlexProcessingDemo/CostCalculator.cs](https://github.com/TallesValiatti/MsftFoundryFlexProcessing/blob/main/src/FlexProcessingDemo/CostCalculator.cs)
- ProcessingResult Model [https://github.com/TallesValiatti/MsftFoundryFlexProcessing/blob/main/src/FlexProcessingDemo/ProcessingResult.cs](https://github.com/TallesValiatti/MsftFoundryFlexProcessing/blob/main/src/FlexProcessingDemo/ProcessingResult.cs)
- Environment Configuration Example [https://github.com/TallesValiatti/MsftFoundryFlexProcessing/blob/main/.env.example](https://github.com/TallesValiatti/MsftFoundryFlexProcessing/blob/main/.env.example)

Observação para o artigo: há uma divergência que merece revisão. O README lista quatro modelos compatíveis com Flex, mas a tabela da documentação oficial consultada apresenta apenas `gpt-5.6-sol` como modelo suportado. Antes de publicar a lista de compatibilidade, utilize a [tabela oficial de modelos suportados](https://learn.microsoft.com/en-us/azure/foundry/openai/how-to/flex-processing#review-supported-models) como referência.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://github.com\&sz=32)

GitHub

+1



Outro ponto importante é que a documentação de Prompt Caching para a família GPT-5.6 passou a contemplar também tokens de escrita de cache (`cache_write_tokens`). Isso é particularmente relevante para a precisão do `CostCalculator` do seu exemplo.&#x20;

[image](https://www.google.com/s2/favicons?domain=https://learn.microsoft.com\&sz=32)

Microsoft Learn
