import { EventEmitter, Injectable } from '@angular/core';
import { HttpTransportType, HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/Message';

@Injectable({
  providedIn: 'root'
})
export class ChatServiceService {
  private _hubConnection: HubConnection;

  constructor() {
    this.startConnection();
  }

  sendMessage(message: Message) {
    this._hubConnection.invoke('SendMessage', message);
  }

  private startConnection = () => {
    this._hubConnection = new HubConnectionBuilder()
      .withUrl('https://localhost:44399/MessageHub',{
        skipNegotiation:true,
        transport: HttpTransportType.WebSockets
      })
      .configureLogging(LogLevel.Information)
      .build();

    this._hubConnection.on('MessageReceived', (message: Message) => {
      console.log('Message', message);
    });

    this._hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err =>{
        console.log('Error while starting connection: ' + err)
      })
  }
}
