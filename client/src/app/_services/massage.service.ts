import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginationHeaders, getPaginationResult } from './paginationHelper';
import { Message } from '../_models/message';

@Injectable({
  providedIn: 'root'
})
export class MassageService {
  basedUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMessages(pageNumber: number, pageSize: number, container: string){
    let params = getPaginationHeaders(pageNumber,pageSize);
    params = params.append('Container',container);
    return getPaginationResult<Message[]>(this.basedUrl + 'message', params, this.http);
  }

  getMessageThread(username: string){
    return this.http.get<Message[]>(this.basedUrl + 'message/thread/'+username);
  }

  sendMessage(username: string, content: string){
    return this.http.post<Message>(this.basedUrl + 'message',
      {recipientUsername: username, content})
  }

  deleteMessage(id: number){
    return this.http.delete(this.basedUrl + 'message/' + id)
  }
}
