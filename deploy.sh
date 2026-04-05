#!/bin/bash

PROJECT_ID="digital-492322"
SERVICE_NAME="crm-task-management"
REGION="europe-west1"
DATABASE_URL="postgresql://crm_db_d7p6_user:WXbdnnmk0akPx285Qln9BwbLMF4ExUvq@dpg-d78oaepr0fns738oa8j0-a.oregon-postgres.render.com/crm_db_d7p6"

echo "=== Setting project ==="
gcloud config set project $PROJECT_ID

echo "=== Enabling required APIs ==="
gcloud services enable run.googleapis.com cloudbuild.googleapis.com artifactregistry.googleapis.com

echo "=== Deploying to Cloud Run ==="
gcloud run deploy $SERVICE_NAME \
  --source=. \
  --region=$REGION \
  --port=8080 \
  \
  --memory=512Mi \
  --min-instances=0 \
  --max-instances=1

echo "=== Done! ==="
echo "Your app URL:"
gcloud run services describe $SERVICE_NAME --region=$REGION --format="value(status.url)"
