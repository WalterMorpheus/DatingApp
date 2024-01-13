import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginationHeaders, getPaginationResult } from './paginationHelper';
import { Message } from '../_models/message';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from '../_models/user';
import { BehaviorSubject, take } from 'rxjs';
import { group } from '@angular/animations';
import { Group } from '../_models/group';

@Injectable({
  providedIn: 'root'
})
export class MassageService {
  basedUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnetion?: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient) { }

  createHubConnection(user: User, otherUsername: string){
    this.hubConnetion = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

      this.hubConnetion.start().catch(error => console.log(error));

      this.hubConnetion.on('ReceiveMessageThread', messages => {
          this.messageThreadSource.next(messages);
      })

      this.hubConnetion.on('UpdatedGroup',(group: Group)=>{
        if(group.connections.some(x => x.username === otherUsername)){
          this.messageThread$.pipe(take(1)).subscribe({
            next: messages =>{
              messages.forEach( message => {
                if(!message.dateRead){
                  message.dateRead = new Date(Date.now())
                }
              })
              this.messageThreadSource.next([...messages]);

            }
          })
        }
      })

      this.hubConnetion.on('NewMessage', message =>{
        this.messageThread$.pipe(take(1)).subscribe({
          next: messages => {
            this.messageThreadSource.next([...messages,message])
          }
        })
      })
  }

  stopHubConnection(){
    if(this.hubConnetion){
      this.hubConnetion.stop();
    }
  }


  getMessages(pageNumber: number, pageSize: number, container: string){
    let params = getPaginationHeaders(pageNumber,pageSize);
    params = params.append('Container',container);
    return getPaginationResult<Message[]>(this.basedUrl + 'message', params, this.http);
  }

  getMessageThread(username: string){
    return this.http.get<Message[]>(this.basedUrl + 'message/thread/'+username);
  }

  async sendMessage(username: string, content: string){
    return this.hubConnetion?.invoke('SendMessage',{recipientUsername: username, content}).
      catch(error => console.log(error));
  }

  deleteMessage(id: number){
    return this.http.delete(this.basedUrl + 'message/' + id)
  }
}
