import { Component, OnInit } from '@angular/core';
import { take } from 'rxjs';
import { Observable } from 'rxjs/internal/Observable';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  //members$: Observable<Member[]> | undefined;
  members: Member[] = [];
  pagination: Pagination | undefined;
  userParams: UserParams| undefined;
  user: User|undefined;
  genderList = [{value: 'male',display:'Male'},{value:'female',display:'Female'}]
  

  constructor(private memberSerivce: MembersService){
    this.userParams = this.memberSerivce.getUserParams();
  }

  ngOnInit(): void {
    //this.members$ = this.memberSerivce.getMembers();
    this.loadMembers();
  }

  loadMembers(){
    if(this.userParams){
      this.memberSerivce.setUserParams(this.userParams);
      this.memberSerivce.getMembers(this.userParams).subscribe({
        next: response =>{
          if(response.result && response.pagination){
            this.members = response.result;
            this.pagination = response.pagination;
  
            console.log(this.pagination);
          }
        }
      })
    }
  }

  restFilters(){
      this.userParams = this.memberSerivce.resetUserParams();
      this.loadMembers();
  }

  pageChanged(event:any){
    if(this.userParams && this.userParams?.pageNumber !== event.page){
      this.userParams.pageNumber = event.page;
      this.memberSerivce.setUserParams(this.userParams);
      this.loadMembers();
    }
  }
}
