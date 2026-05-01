This Terraform module provisions AWS resources used by the AI Workflow Board:

- S3 bucket for generated images
- SQS queue for generation jobs

Usage
1. Configure AWS credentials (environment variables, profile, or IAM role).

2. From this folder run:

```bash
terraform init
terraform plan -var="s3_bucket=your-unique-bucket-name" -out=tfplan
terraform apply "tfplan"
```

Recommended variables
- `s3_bucket` (required): unique S3 bucket name.
- `region` (optional): AWS region (default `us-east-1`).
- `s3_public_read` (optional): set to `true` only if you want objects to be publicly readable (recommended: keep false and use presigned URLs or CDN).
- `tags` (optional): map of tags to apply.

Outputs
- `s3_bucket`: bucket name
- `s3_bucket_arn`: bucket ARN
- `sqs_queue_url`: SQS queue URL
- `sqs_queue_arn`: SQS queue ARN

Security notes
- Avoid enabling `s3_public_read` for production-sensitive images. Use private bucket + presigned URLs or CloudFront with signed cookies.
- Do not store AWS credentials in the repo or commit them to Git.

Integration
- Add the outputs `s3_bucket` and `sqs_queue_url` as `Aws:S3Bucket` and `Aws:SqsQueueUrl` in your backend `appsettings` or CI secrets.

Example:
```hcl
terraform apply -var="s3_bucket=ai-workflow-board-generated-12345" -var="s3_public_read=false"
```

GitHub Actions (OIDC) deploy
---------------------------------
You can let GitHub Actions run Terraform securely without long-lived AWS keys using OIDC.

Workflow setup summary:
- Run `terraform apply` locally once (or create the role manually) so the IAM Role exists in your account.
- Create a repository secret `AWS_ROLE_ARN` with the role ARN (format: `arn:aws:iam::801366809138:role/ai-workflow-board-github-actions`).
- Create a repository secret `S3_BUCKET_NAME` with the bucket name you want Terraform to create.
- (Optional) create `AWS_REGION` secret if you don't want `us-east-1`.

The workflow `.github/workflows/deploy-infra-oidc.yml` will assume the role via OIDC and run `terraform plan` / `apply`.

Notes on bootstrapping:
- Because the workflow assumes the role, the role must already exist (or you must create it manually first). The Terraform files include `iam_oidc.tf` which can create the role locally.
- Recommended sequence when enabling CI for the first time:
	1. Run Terraform locally with your AWS credentials to create the bucket, queue and the `github_actions` role:

```bash
cd infra/terraform
terraform init
terraform apply -var="s3_bucket=your-unique-bucket-name" -var="github_repository=your-org/your-repo"
```

	2. Copy the role ARN shown in the Terraform output (or from the AWS console) and set it as the `AWS_ROLE_ARN` repository secret.
	3. Push to `main` or dispatch the `Deploy Infra (Terraform via GitHub OIDC)` workflow.

