output "sqs_queue_url" {
  value = aws_sqs_queue.generation.id
}

output "s3_bucket" {
  value = aws_s3_bucket.images.id
}

output "s3_bucket_arn" {
  value = aws_s3_bucket.images.arn
}

output "sqs_queue_arn" {
  value = aws_sqs_queue.generation.arn
}
