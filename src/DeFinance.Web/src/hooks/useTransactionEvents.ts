import { useEffect } from 'react'
import * as signalR from '@microsoft/signalr'
import { TOKEN_KEY } from '../api/auth'

export function useTransactionEvents(onChanged: () => void) {
  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/notifications', {
        accessTokenFactory: () => localStorage.getItem(TOKEN_KEY) ?? '',
      })
      .withAutomaticReconnect()
      .build()

    connection.on('TransactionChanged', onChanged)

    connection.start().catch(() => {})

    return () => { connection.stop() }
  }, [onChanged])
}
