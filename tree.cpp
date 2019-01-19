#include <iostream>
#include <vector>
#include <cmath>
using namespace std;
vector <double> d;
void hash_tree(vector <double> c){
    vector <double> new_c;
  for (int i = 0; i < c.size()-1; i += 2)
    {
       double a = pow(2, (c[i]) / 10) + pow(2, (c[i + 1]) / 10);
        d.push_back(a);
        new_c.push_back(a);
    }
    if(new_c.size() != 1){
        hash_tree(new_c);
    }

}
int main(){
int n;
cin>>n;
//enter data array
for(int i=0;i<n;i++){
    double a;
    cin>>a;
    d.push_back(a);
}
hash_tree(d);

//hash tree
for(int i=0;i<d.size();i++){
    cout<<d[i]<<" ";
}

return 0;
}
