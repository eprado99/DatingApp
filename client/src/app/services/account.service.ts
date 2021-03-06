import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  baseUrl: string = "https://localhost:5001/api/";
  private currentUserSource = new ReplaySubject<User>(1);
  currentUser$ = this.currentUserSource.asObservable();
  
  constructor(private http: HttpClient) { }

  login(model: any){
    return this.http.post(this.baseUrl + "account/login", model).pipe(
      map((res: User) => {
        const user = res;
        if(user){
          localStorage.setItem("user", JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    );
  }

  register(model: any){
    return this.http.post(this.baseUrl + "account/register", model).pipe(
      map((res : User) => {
        if(res) {
          localStorage.setItem("user", JSON.stringify(res));
          this.currentUserSource.next(res);
        }
      })
    )
  }
  setCurrentUser(user: User){
    this.currentUserSource.next(user);
  }
  logout(){
    localStorage.removeItem("user");
    this.currentUserSource.next(null);
  }
}
