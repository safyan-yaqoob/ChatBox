import { EventEmitter, Injectable } from '@angular/core';
import { HttpTransportType, HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/Message';

@Injectable({
  providedIn: 'root'
})
export class ChatServiceService {
  private _hubConnection: HubConnection;
  connectionId:string;
  messageReceived = new EventEmitter<Message>();  

  constructor() {
    this.startConnection();
    this.registerOnServerEvents();
  }

  async sendMessage(message: Message) {
    await this._hubConnection.invoke('SendMessage', message,this.connectionId);
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
      .then(() => this.getConnectionId())
      .catch(err =>{
        console.log('Error while starting connection: ' + err)
      })
  }

  public getConnectionId = () => {
    this._hubConnection.invoke('getconnectionid').then(
      (data) => {
          this.connectionId = data;
        }
    ); 
  }

  private registerOnServerEvents(){
    this._hubConnection.on('MessageReceived',(data:any)=>{
      this.messageReceived.emit(data);
    })
  }
}
