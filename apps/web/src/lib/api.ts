const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

async function createApiError(response: Response): Promise<ApiError> {
  let message = `API request failed with status ${response.status}`

  try {
    const body = (await response.json()) as {
      message?: string
      Message?: string
    }

    message = body.message ?? body.Message ?? message
  } catch {
    // Keep the fallback HTTP status message.
  }

  return new ApiError(response.status, message)
}

export async function apiGet<T>(
  path: string,
  signal?: AbortSignal,
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    signal,
    headers: {
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw await createApiError(response)
  }

  return response.json() as Promise<T>
}

export async function apiPost<TResponse, TRequest>(
  path: string,
  body: TRequest,
  signal?: AbortSignal,
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    signal,
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    throw await createApiError(response)
  }

  return response.json() as Promise<TResponse>
}

export async function apiPostForm<TResponse>(
  path: string,
  formData: FormData,
  signal?: AbortSignal,
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    signal,
    headers: {
      Accept: 'application/json',
    },
    body: formData,
  })

  if (!response.ok) {
    throw await createApiError(response)
  }

  return response.json() as Promise<TResponse>
}

export { API_BASE_URL }