terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 4.0"
    }
  }
}

provider "aws" {
  region = var.region
}

resource "aws_s3_bucket" "images" {
  bucket = var.s3_bucket
  acl    = var.s3_public_read ? "public-read" : "private"

  versioning {
    enabled = true
  }

  tags = var.tags
}

resource "aws_sqs_queue" "generation" {
  name                      = var.sqs_queue_name
  visibility_timeout_seconds = 300
  message_retention_seconds  = 86400

  tags = var.tags
}

// Optional public-read bucket policy when s3_public_read is true
resource "aws_s3_bucket_policy" "public_read" {
  count  = var.s3_public_read ? 1 : 0
  bucket = aws_s3_bucket.images.id

  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Sid       = "AllowPublicReadGetObject",
        Effect    = "Allow",
        Principal = "*",
        Action    = ["s3:GetObject"],
        Resource  = ["${aws_s3_bucket.images.arn}/*"]
      }
    ]
  })
}

// Outputs moved to outputs.tf
