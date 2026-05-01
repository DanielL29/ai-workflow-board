variable "region" {
  type    = string
  default = "us-east-1"
}

variable "s3_bucket" {
  type = string
}

variable "sqs_queue_name" {
  type    = string
  default = "ai-workflow-board-generation-queue"
}

variable "s3_public_read" {
  type    = bool
  default = false
}

variable "tags" {
  type = map(string)
  default = {}
}

variable "github_repository" {
  type = string
  description = "GitHub repository in the form 'owner/repo' used for OIDC subject restriction."
}

variable "github_branch" {
  type    = string
  default = "main"
  description = "Branch allowed to assume the role (used in subject restriction)."
}

variable "github_actions_role_name" {
  type    = string
  default = "ai-workflow-board-github-actions"
  description = "Name of the IAM role to create for GitHub Actions OIDC assumption."
}
